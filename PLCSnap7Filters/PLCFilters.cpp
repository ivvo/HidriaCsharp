#include "UserFilter.h"
#include "AVL_Lite.h"
#include "UserFilterLibrary.hxx"
#include "snap7.h"
#include "windows.h"

using namespace avs;

namespace avs
{
    // ─────────────────────────────
    // PLC ERROR CLASSIFICATION
    // ─────────────────────────────
    // 0=None, 1=ConnectionLost, 2=ReadFailed, 3=WriteFailed, 4=Timeout
    enum class PLCErrorType
    {
        None = 0,
        ConnectionLost = 1,
        ReadFailed = 2,
        WriteFailed = 3,
        Timeout = 4
    };

    // ─────────────────────────────
    // GLOBAL PLC
    // ─────────────────────────────
    class PLCManager
    {
    public:
        static S7Object client;
        static bool connected;
        static PLCErrorType lastError;
        static DWORD lastSuccessTick; // GetTickCount() ob zadnjem uspešnem branju/pisanju
    };

    S7Object PLCManager::client = 0;
    bool PLCManager::connected = false;
    PLCErrorType PLCManager::lastError = PLCErrorType::None;
    DWORD PLCManager::lastSuccessTick = 0;

    // Pomožne funkcije za enotno klasifikacijo napak čez vse filtre.
    static inline void MarkConnectFailure(int res)
    {
        PLCManager::lastError = (res == (int)errCliJobTimeout) ? PLCErrorType::Timeout : PLCErrorType::ConnectionLost;
    }

    static inline void MarkReadFailure(int res)
    {
        PLCManager::connected = false;
        PLCManager::lastError = (res == (int)errCliJobTimeout) ? PLCErrorType::Timeout : PLCErrorType::ReadFailed;
    }

    static inline void MarkWriteFailure(int res)
    {
        PLCManager::connected = false;
        PLCManager::lastError = (res == (int)errCliJobTimeout) ? PLCErrorType::Timeout : PLCErrorType::WriteFailed;
    }

    static inline void MarkSuccess()
    {
        PLCManager::lastError = PLCErrorType::None;
        PLCManager::lastSuccessTick = GetTickCount();
    }

    static inline const wchar_t* ErrorTypeToText(PLCErrorType t)
    {
        switch (t)
        {
            case PLCErrorType::ConnectionLost: return L"Connection Lost";
            case PLCErrorType::ReadFailed:     return L"Read Failed";
            case PLCErrorType::WriteFailed:    return L"Write Failed";
            case PLCErrorType::Timeout:        return L"Timeout";
            default:                           return L"None";
        }
    }

    // ─────────────────────────────
    // CONNECT
    // ─────────────────────────────
    class HidriaPLCConnect : public UserFilter
    {
    private:
        int reconnectCounter = 0;

    public:
        void Define() override
        {
            SetName(L"HidriaPLCConnect");
            SetCategory(L"Hidria");

            AddInput(L"inEnable", L"Bool", L"False", L"Enable");

            AddInput(L"inIP", L"String", L"192.168.0.10", L"IP");
            AddInput(L"inRack", L"Integer", L"0", L"Rack");
            AddInput(L"inSlot", L"Integer", L"1", L"Slot");

            AddOutput(L"outConnected", L"Integer", L"Status");
        }

        int Invoke() override
        {
            bool enable;
            atl::String ipStr;
            int rack, slot;

            ReadInput(L"inEnable", enable);
            ReadInput(L"inIP", ipStr);
            ReadInput(L"inRack", rack);
            ReadInput(L"inSlot", slot);

            std::string ip(ipStr.Begin(), ipStr.End());

            // ───────── DISABLE = FORCE DISCONNECT ─────────
            if (!enable)
            {
                if (PLCManager::connected && PLCManager::client)
                {
                    Cli_Disconnect(PLCManager::client);
                    Cli_Destroy(&PLCManager::client);
                    PLCManager::client = 0;
                    PLCManager::connected = false;
                }

                reconnectCounter = 0;
                PLCManager::lastError = PLCErrorType::None;

                WriteOutput(L"outConnected", 0);
                return INVOKE_NORMAL;
            }

            // ───────── CONNECTION LOST → TRY RECONNECT ─────────
            if (!PLCManager::connected)
            {
                reconnectCounter++;

                // reconnect every 10 cycles (≈ 1s če Timer = 100 ms)
                if (reconnectCounter >= 10)
                {
                    reconnectCounter = 0;

                    if (!PLCManager::client)
                        PLCManager::client = Cli_Create();

                    int res = Cli_ConnectTo(PLCManager::client, ip.c_str(), rack, slot);

                    PLCManager::connected = (res == 0);

                    if (PLCManager::connected)
                        MarkSuccess();
                    else
                        MarkConnectFailure(res);
                }
            }

            WriteOutput(L"outConnected", PLCManager::connected ? 1 : 0);

            return INVOKE_NORMAL;
        }
    };
    // ─────────────────────────────
    // PLC CONNECTION STATUS → SAFE BIT WRITE
    // bit0 = Offline
    // bit1 = Online (heartbeat)
    // bit2 = Error
    // ─────────────────────────────
    class HidriaPLCConnectionStatus : public UserFilter
    {
    private:
        bool heartbeat = false;
        DWORD lastToggle = 0;

    public:
        void Define() override
        {
            SetName(L"HidriaPLCConnectionStatus");
            SetCategory(L"Hidria");

            AddInput(L"inEnable", L"Integer", L"1", L"Enable (Online/Offline)");
            AddInput(L"inError", L"Bool", L"False", L"Manual error");

            AddInput(L"inDB", L"Integer", L"1", L"DB");
            AddInput(L"inOffset", L"Integer", L"0", L"Byte offset");

            AddInput(L"inPeriodMs", L"Integer", L"500", L"Heartbeat period (ms)");

            AddOutput(L"outOffline", L"Bool", L"Offline");
            AddOutput(L"outOnline", L"Bool", L"Online (heartbeat)");
            AddOutput(L"outError", L"Bool", L"Error");

            // Podrobnejša klasifikacija napake + čas od zadnjega uspešnega branja/pisanja.
            AddOutput(L"outErrorType", L"Integer", L"0=None,1=ConnectionLost,2=ReadFailed,3=WriteFailed,4=Timeout");
            AddOutput(L"outErrorText", L"String", L"Human-readable error type");
            AddOutput(L"outSecondsSinceLastSuccess", L"Real", L"Seconds since last successful PLC read/write");
        }

        int Invoke() override
        {
            int enable, db, offset, periodMs;
            bool error;

            ReadInput(L"inEnable", enable);
            ReadInput(L"inError", error);
            ReadInput(L"inDB", db);
            ReadInput(L"inOffset", offset);
            ReadInput(L"inPeriodMs", periodMs);

            bool offline = !enable;
            bool connected = PLCManager::connected;

            // ✅ HEARTBEAT (time-based)
            DWORD now = GetTickCount();

            if (lastToggle == 0)
                lastToggle = now;

            if ((now - lastToggle) >= (DWORD)periodMs)
            {
                heartbeat = !heartbeat;
                lastToggle = now;
            }

            float secondsSinceLastSuccess = (PLCManager::lastSuccessTick == 0)
                ? -1.0f
                : (float)(now - PLCManager::lastSuccessTick) / 1000.0f;

            // ✅ če ni povezan → samo status, brez write
            if (!connected || !PLCManager::client)
            {
                WriteOutput(L"outOffline", offline);
                WriteOutput(L"outOnline", false);
                WriteOutput(L"outError", error);
                WriteOutput(L"outErrorType", (int)PLCManager::lastError);
                WriteOutput(L"outErrorText", atl::String(ErrorTypeToText(PLCManager::lastError)));
                WriteOutput(L"outSecondsSinceLastSuccess", secondsSinceLastSuccess);
                return INVOKE_NORMAL;
            }

            uint8_t buffer[1];

            // ✅ PREBERI OBSTOJEČ BYTE
            int res = Cli_DBRead(PLCManager::client, db, offset, 1, buffer);

            if (res != 0)
            {
                PLCManager::connected = false;
                MarkReadFailure(res);

                WriteOutput(L"outOffline", true);
                WriteOutput(L"outOnline", false);
                WriteOutput(L"outError", true);
                WriteOutput(L"outErrorType", (int)PLCManager::lastError);
                WriteOutput(L"outErrorText", atl::String(ErrorTypeToText(PLCManager::lastError)));
                WriteOutput(L"outSecondsSinceLastSuccess", secondsSinceLastSuccess);

                return INVOKE_NORMAL;
            }

            // ✅ BIT 0 → OFFLINE
            if (offline)
                buffer[0] |= (1 << 0);
            else
                buffer[0] &= ~(1 << 0);

            // ✅ BIT 1 → ONLINE (heartbeat)
            if (enable && connected && heartbeat)
                buffer[0] |= (1 << 1);
            else
                buffer[0] &= ~(1 << 1);

            // ✅ BIT 2 → ERROR
            if (error)
                buffer[0] |= (1 << 2);
            else
                buffer[0] &= ~(1 << 2);

            // ✅ ZAPIŠI NAZAJ
            res = Cli_DBWrite(PLCManager::client, db, offset, 1, buffer);

            if (res != 0)
            {
                PLCManager::connected = false;
                MarkWriteFailure(res);

                WriteOutput(L"outOffline", true);
                WriteOutput(L"outOnline", false);
                WriteOutput(L"outError", true);
                WriteOutput(L"outErrorType", (int)PLCManager::lastError);
                WriteOutput(L"outErrorText", atl::String(ErrorTypeToText(PLCManager::lastError)));
                WriteOutput(L"outSecondsSinceLastSuccess", secondsSinceLastSuccess);

                return INVOKE_NORMAL;
            }

            MarkSuccess();
            secondsSinceLastSuccess = 0.0f;

            // ✅ OUTPUTI (HMI)
            WriteOutput(L"outOffline", offline);
            WriteOutput(L"outOnline", (enable && connected) ? heartbeat : false);
            WriteOutput(L"outError", error);
            WriteOutput(L"outErrorType", (int)PLCManager::lastError);
            WriteOutput(L"outErrorText", atl::String(ErrorTypeToText(PLCManager::lastError)));
            WriteOutput(L"outSecondsSinceLastSuccess", secondsSinceLastSuccess);

            return INVOKE_NORMAL;
        }
    };
    // ─────────────────────────────
    // PLC Read Bits
    // ─────────────────────────────
    class HidriaPLCReadBits : public UserFilter
    {
    public:
        void Define() override
        {
            SetName(L"HidriaPLCReadBits");
            SetCategory(L"Hidria");

            AddInput(L"inEnable", L"Integer", L"1", L"Enable");
            AddInput(L"inDB", L"Integer", L"1", L"DB");
            AddInput(L"inOffset", L"Integer", L"0", L"Byte offset");

            // 8 bool outputov ✅
            AddOutput(L"outBit0", L"Bool", L"Bit 0");
            AddOutput(L"outBit1", L"Bool", L"Bit 1");
            AddOutput(L"outBit2", L"Bool", L"Bit 2");
            AddOutput(L"outBit3", L"Bool", L"Bit 3");
            AddOutput(L"outBit4", L"Bool", L"Bit 4");
            AddOutput(L"outBit5", L"Bool", L"Bit 5");
            AddOutput(L"outBit6", L"Bool", L"Bit 6");
            AddOutput(L"outBit7", L"Bool", L"Bit 7");

            AddOutput(L"outConnected", L"Integer", L"Status");
        }

        int Invoke() override
        {
            int enable, db, offset;

            ReadInput(L"inEnable", enable);
            ReadInput(L"inDB", db);
            ReadInput(L"inOffset", offset);

            // če ni omogočeno ali ni povezave
            if (!enable || !PLCManager::connected)
            {
                WriteOutput(L"outBit0", false);
                WriteOutput(L"outBit1", false);
                WriteOutput(L"outBit2", false);
                WriteOutput(L"outBit3", false);
                WriteOutput(L"outBit4", false);
                WriteOutput(L"outBit5", false);
                WriteOutput(L"outBit6", false);
                WriteOutput(L"outBit7", false);

                WriteOutput(L"outConnected", 0);
                return INVOKE_NORMAL;
            }

            uint8_t buffer[1];

            int res = Cli_DBRead(PLCManager::client, db, offset, 1, buffer);

            if (res != 0)
            {
                MarkReadFailure(res);

                WriteOutput(L"outConnected", 0);
                return INVOKE_NORMAL;
            }

            MarkSuccess();

            uint8_t value = buffer[0];

            // razbij na bit-e → BOOL ✅
            WriteOutput(L"outBit0", ((value >> 0) & 1) != 0);
            WriteOutput(L"outBit1", ((value >> 1) & 1) != 0);
            WriteOutput(L"outBit2", ((value >> 2) & 1) != 0);
            WriteOutput(L"outBit3", ((value >> 3) & 1) != 0);
            WriteOutput(L"outBit4", ((value >> 4) & 1) != 0);
            WriteOutput(L"outBit5", ((value >> 5) & 1) != 0);
            WriteOutput(L"outBit6", ((value >> 6) & 1) != 0);
            WriteOutput(L"outBit7", ((value >> 7) & 1) != 0);

            WriteOutput(L"outConnected", 1);

            return INVOKE_NORMAL;
        }
    };

  // ─────────────────────────────
  // PLC WRITE BITS (8 BOOL inputov → 1 byte)
  // ─────────────────────────────
    class HidriaPLCWriteBits : public UserFilter
    {
    public:
        void Define() override
        {
            SetName(L"HidriaPLCWriteBits");
            SetCategory(L"Hidria");

            AddInput(L"inEnable", L"Integer", L"1", L"Enable");
            AddInput(L"inDB", L"Integer", L"1", L"DB");
            AddInput(L"inOffset", L"Integer", L"0", L"Byte offset");

            // 8 vhodnih bitov ✅
            AddInput(L"inBit0", L"Bool", L"False", L"Bit 0");
            AddInput(L"inBit1", L"Bool", L"False", L"Bit 1");
            AddInput(L"inBit2", L"Bool", L"False", L"Bit 2");
            AddInput(L"inBit3", L"Bool", L"False", L"Bit 3");
            AddInput(L"inBit4", L"Bool", L"False", L"Bit 4");
            AddInput(L"inBit5", L"Bool", L"False", L"Bit 5");
            AddInput(L"inBit6", L"Bool", L"False", L"Bit 6");
            AddInput(L"inBit7", L"Bool", L"False", L"Bit 7");

            AddOutput(L"outConnected", L"Integer", L"Status");
        }

        int Invoke() override
        {
            int enable, db, offset;
            bool b0, b1, b2, b3, b4, b5, b6, b7;

            ReadInput(L"inEnable", enable);
            ReadInput(L"inDB", db);
            ReadInput(L"inOffset", offset);

            ReadInput(L"inBit0", b0);
            ReadInput(L"inBit1", b1);
            ReadInput(L"inBit2", b2);
            ReadInput(L"inBit3", b3);
            ReadInput(L"inBit4", b4);
            ReadInput(L"inBit5", b5);
            ReadInput(L"inBit6", b6);
            ReadInput(L"inBit7", b7);

            if (!enable || !PLCManager::connected)
            {
                WriteOutput(L"outConnected", 0);
                return INVOKE_NORMAL;
            }

            // sestavi byte iz bitov
            uint8_t value = 0;

            if (b0) value |= (1 << 0);
            if (b1) value |= (1 << 1);
            if (b2) value |= (1 << 2);
            if (b3) value |= (1 << 3);
            if (b4) value |= (1 << 4);
            if (b5) value |= (1 << 5);
            if (b6) value |= (1 << 6);
            if (b7) value |= (1 << 7);

            uint8_t buffer[1];
            buffer[0] = value;

            int res = Cli_DBWrite(PLCManager::client, db, offset, 1, buffer);

            if (res != 0)
            {
                MarkWriteFailure(res);
                WriteOutput(L"outConnected", 0);
                return INVOKE_NORMAL;
            }

            MarkSuccess();

            WriteOutput(L"outConnected", 1);
            return INVOKE_NORMAL;
        }
    };
    // ─────────────────────────────
    // HIDRIA PLC READ BYTE
    // ─────────────────────────────
    class HidriaPLCReadByte : public UserFilter
    {
    public:
        void Define() override
        {
            SetName(L"HidriaPLCReadByte");
            SetCategory(L"Hidria");

            AddInput(L"inEnable", L"Integer", L"1", L"Enable");
            AddInput(L"inDB", L"Integer", L"1", L"DB");
            AddInput(L"inOffset", L"Integer", L"0", L"Byte offset");

            AddOutput(L"outValue", L"Integer", L"Byte value (0-255)");
            AddOutput(L"outConnected", L"Integer", L"Status");
        }

        int Invoke() override
        {
            int enable, db, offset;

            ReadInput(L"inEnable", enable);
            ReadInput(L"inDB", db);
            ReadInput(L"inOffset", offset);

            if (!enable || !PLCManager::connected || !PLCManager::client)
            {
                WriteOutput(L"outValue", 0);
                WriteOutput(L"outConnected", 0);
                return INVOKE_NORMAL;
            }

            uint8_t buffer[1];

            int res = Cli_DBRead(PLCManager::client, db, offset, 1, buffer);

            if (res != 0)
            {
                MarkReadFailure(res);

                WriteOutput(L"outValue", 0);
                WriteOutput(L"outConnected", 0);
                return INVOKE_NORMAL;
            }

            MarkSuccess();

            WriteOutput(L"outValue", (int)buffer[0]);
            WriteOutput(L"outConnected", 1);

            return INVOKE_NORMAL;
        }
    };
    // ─────────────────────────────
    // HIDRIA PLC WRITE BYTE
    // ─────────────────────────────
    class HidriaPLCWriteByte : public UserFilter
    {
    public:
        void Define() override
        {
            SetName(L"HidriaPLCWriteByte");
            SetCategory(L"Hidria");

            AddInput(L"inEnable", L"Integer", L"1", L"Enable");
            AddInput(L"inDB", L"Integer", L"1", L"DB");
            AddInput(L"inOffset", L"Integer", L"0", L"Byte offset");
            AddInput(L"inValue", L"Integer", L"0", L"Byte value (0-255)");

            AddOutput(L"outConnected", L"Integer", L"Status");
        }

        int Invoke() override
        {
            int enable, db, offset, value;

            ReadInput(L"inEnable", enable);
            ReadInput(L"inDB", db);
            ReadInput(L"inOffset", offset);
            ReadInput(L"inValue", value);

            if (!enable || !PLCManager::connected || !PLCManager::client)
            {
                WriteOutput(L"outConnected", 0);
                return INVOKE_NORMAL;
            }

            // clamp vrednosti
            if (value < 0) value = 0;
            if (value > 255) value = 255;

            uint8_t buffer[1];
            buffer[0] = (uint8_t)value;

            int res = Cli_DBWrite(PLCManager::client, db, offset, 1, buffer);

            if (res != 0)
            {
                MarkWriteFailure(res);
                WriteOutput(L"outConnected", 0);
                return INVOKE_NORMAL;
            }

            MarkSuccess();

            WriteOutput(L"outConnected", 1);

            return INVOKE_NORMAL;
        }
    };
    // ─────────────────────────────
    // READ FLOAT
    // ─────────────────────────────
    class HidriaPLCReadFloat : public UserFilter
    {
    public:
        void Define() override
        {
            SetName(L"HidriaPLCReadFloat");
            SetCategory(L"Hidria");

            AddInput(L"inEnable", L"Integer", L"1", L"Enable");
            AddInput(L"inDB", L"Integer", L"2", L"DB");
            AddInput(L"inOffset", L"Integer", L"0", L"Offset");

            AddOutput(L"outFloat", L"Real", L"Float value");
            AddOutput(L"outConnected", L"Integer", L"Status");
        }

        int Invoke() override
        {
            int enable, db, offset;

            ReadInput(L"inEnable", enable);
            ReadInput(L"inDB", db);
            ReadInput(L"inOffset", offset);

            if (!enable || !PLCManager::connected)
            {
                WriteOutput(L"outConnected", 0);
                return INVOKE_NORMAL;
            }

            uint8_t buffer[4];

            int res = Cli_DBRead(PLCManager::client, db, offset, 4, buffer);

            if (res != 0)
            {
                MarkReadFailure(res);
                WriteOutput(L"outConnected", 0);
                return INVOKE_NORMAL;
            }

            MarkSuccess();

            // BIG ENDIAN → FLOAT
            uint32_t raw =
                (buffer[0] << 24) |
                (buffer[1] << 16) |
                (buffer[2] << 8) |
                buffer[3];

            float value;
            std::memcpy(&value, &raw, sizeof(float));

            WriteOutput(L"outFloat", value);
            WriteOutput(L"outConnected", 1);

            return INVOKE_NORMAL;
        }
    };

    // ─────────────────────────────
    // WRITE FLOAT ✅
    // ─────────────────────────────
    class HidriaPLCWriteFloat : public UserFilter
    {
    public:
        void Define() override
        {
            SetName(L"HidriaPLCWriteFloat");
            SetCategory(L"Hidria");

            AddInput(L"inEnable", L"Integer", L"1", L"Enable");
            AddInput(L"inDB", L"Integer", L"2", L"DB");
            AddInput(L"inOffset", L"Integer", L"0", L"Offset");
            AddInput(L"inValue", L"Real", L"0.0", L"Float value");

            AddOutput(L"outConnected", L"Integer", L"Status");
        }

        int Invoke() override
        {
            int enable, db, offset;
            float value;

            ReadInput(L"inEnable", enable);
            ReadInput(L"inDB", db);
            ReadInput(L"inOffset", offset);
            ReadInput(L"inValue", value);

            if (!enable || !PLCManager::connected)
            {
                WriteOutput(L"outConnected", 0);
                return INVOKE_NORMAL;
            }

            uint32_t raw;
            std::memcpy(&raw, &value, sizeof(float));

            uint8_t buffer[4];

            // FLOAT → BIG ENDIAN
            buffer[0] = (raw >> 24) & 0xFF;
            buffer[1] = (raw >> 16) & 0xFF;
            buffer[2] = (raw >> 8) & 0xFF;
            buffer[3] = raw & 0xFF;

            int res = Cli_DBWrite(PLCManager::client, db, offset, 4, buffer);

            if (res != 0)
            {
                MarkWriteFailure(res);
                WriteOutput(L"outConnected", 0);
                return INVOKE_NORMAL;
            }

            MarkSuccess();

            WriteOutput(L"outConnected", 1);
            return INVOKE_NORMAL;
        }
    };

    // ─────────────────────────────
    // REGISTER
    // ─────────────────────────────
    class RegisterUserObjects
    {
    public:
        RegisterUserObjects()
        {
            RegisterFilter(CreateInstance<HidriaPLCConnectionStatus>);
            RegisterFilter(CreateInstance<HidriaPLCConnect>);
            RegisterFilter(CreateInstance<HidriaPLCReadFloat>);
            RegisterFilter(CreateInstance<HidriaPLCWriteFloat>);
            RegisterFilter(CreateInstance<HidriaPLCReadBits>);
            RegisterFilter(CreateInstance<HidriaPLCWriteBits>);
            RegisterFilter(CreateInstance<HidriaPLCReadByte >);
            RegisterFilter(CreateInstance<HidriaPLCWriteByte>);
        }
    };

    static RegisterUserObjects registerUserObjects;
}
