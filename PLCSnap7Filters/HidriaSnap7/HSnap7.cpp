#include "HSnap7.h"
#include <cstring>   // memcpy
#include <stdexcept>
#include <typeindex>

namespace Snap7Manager
{

// ── Konstruktor ──────────────────────────────────────────────────────────────

HSnap7::HSnap7()
{
    Client = Cli_Create();
}

// ── Javne metode ─────────────────────────────────────────────────────────────

void HSnap7::PlcConnect(const std::string& address, int rack, int slot)
{
    int res = Cli_ConnectTo(Client, address.c_str(), rack, slot);
    if (res != 0)
        throw Snap7Exception(ERR_CONNECTING);
}

void HSnap7::PlcDisconnect()
{
    Cli_Disconnect(Client);
}

std::map<std::string, std::vector<DynamicValue>>
HSnap7::GetSections(const DBSection& dbSections)
{
    SectionsData.clear();

    std::vector<uint8_t> tmpData = ReadPLC(dbSections);

    for (const DBData& data : dbSections.Data)
    {
        std::string nameAndType = data.Name + ":" + data.DataType.name();
        std::vector<DynamicValue> values;

        if (data.DataType == typeid(uint8_t))
        {
            for (auto v : ReadValuesByte(tmpData, data.StartAddress, data.Length))
                values.emplace_back(v);
        }
        else if (data.DataType == typeid(int16_t))
        {
            for (auto v : ReadValuesInt16(tmpData, data.StartAddress, data.Length))
                values.emplace_back(v);
        }
        else if (data.DataType == typeid(int32_t))
        {
            for (auto v : ReadValuesInt32(tmpData, data.StartAddress, data.Length))
                values.emplace_back(v);
        }
        else if (data.DataType == typeid(float))
        {
            for (auto v : ReadValuesFloat(tmpData, data.StartAddress, data.Length))
                values.emplace_back(v);
        }

        SectionsData[nameAndType] = std::move(values);
    }

    return SectionsData;
}

void HSnap7::SetSections(
    const std::map<std::string, std::vector<DynamicValue>>& valuesAndNames,
    const DBSection& dbSections)
{
    int dbNumber       = dbSections.DBNumber;
    int byteLength     = ComputeByteLength(dbSections);
    if (byteLength == 0) byteLength = 1;

    std::vector<uint8_t> outBuffer(byteLength, 0);

    for (const DBData& data : dbSections.Data)
    {
        std::string nameAndType = data.Name + ":" + data.DataType.name();
        const auto& vals = valuesAndNames.at(nameAndType);

        if (data.DataType == typeid(uint8_t))
            WriteValuesByte (data.StartAddress, data.Length, outBuffer, vals);
        else if (data.DataType == typeid(int16_t))
            WriteValuesInt16(data.StartAddress, data.Length, outBuffer, vals);
        else if (data.DataType == typeid(int32_t))
            WriteValuesInt32(data.StartAddress, data.Length, outBuffer, vals);
        else if (data.DataType == typeid(float))
            WriteValuesFloat(data.StartAddress, data.Length, outBuffer, vals);
    }

    WritePLC(outBuffer, dbNumber, byteLength);
}

std::map<std::string, std::vector<DynamicValue>>
HSnap7::CreateDefaultDictionary(const DBSection& dbSections)
{
    std::map<std::string, std::vector<DynamicValue>> tempData;

    for (const DBData& data : dbSections.Data)
    {
        std::string nameAndType = data.Name + ":" + data.DataType.name();
        std::vector<DynamicValue> values;

        if (data.DataType == typeid(uint8_t))
        {
            for (auto v : CreateDefaultsByte(data.Length))
                values.emplace_back(v);
        }
        else if (data.DataType == typeid(int16_t))
        {
            for (auto v : CreateDefaultsInt16(data.Length))
                values.emplace_back(v);
        }
        else if (data.DataType == typeid(int32_t))
        {
            for (auto v : CreateDefaultsInt32(data.Length))
                values.emplace_back(v);
        }
        else if (data.DataType == typeid(float))
        {
            for (auto v : CreateDefaultsFloat(data.Length))
                values.emplace_back(v);
        }

        tempData[nameAndType] = std::move(values);
    }

    return tempData;
}

// ── Zasebne metode ────────────────────────────────────────────────────────────

bool HSnap7::IsLittleEndian()
{
    uint16_t test = 1;
    return *reinterpret_cast<uint8_t*>(&test) == 1;
}

void HSnap7::ReverseBytes(uint8_t* bytes, int count)
{
    for (int i = 0, j = count - 1; i < j; ++i, --j)
        std::swap(bytes[i], bytes[j]);
}

int HSnap7::ComputeByteLength(const DBSection& dbSections) const
{
    int largestAddr     = 0;
    int largestLen      = 0;
    std::type_index largestType(typeid(uint8_t));

    for (const DBData& data : dbSections.Data)
    {
        if (data.StartAddress > largestAddr)
        {
            largestAddr  = data.StartAddress;
            largestLen   = data.Length;
            largestType  = data.DataType;
        }
    }

    int typeSize = 1;
    if      (largestType == typeid(uint8_t))  typeSize = 1;
    else if (largestType == typeid(int16_t))  typeSize = 2;
    else if (largestType == typeid(int32_t))  typeSize = 4;
    else if (largestType == typeid(float))    typeSize = 4;

    return largestAddr + typeSize * largestLen;
}

std::vector<uint8_t> HSnap7::ReadPLC(const DBSection& dbSections)
{
    int byteLength = ComputeByteLength(dbSections);
    std::vector<uint8_t> outBuffer(byteLength, 0);

    int result = Cli_DBRead(Client, dbSections.DBNumber, 0, byteLength, outBuffer.data());

    if (result == 0)
        return outBuffer;
    // 0x00200003 = errCliAddressOutOfRange, 0x00200004 = errCliItemNotAvailable (snap7.h)
    else if (result == 0x00200003 || result == 0x00200004)
        throw Snap7Exception(ERR_READING);
    else
        throw Snap7Exception(ERR_CONNECTION);
}

void HSnap7::WritePLC(const std::vector<uint8_t>& outBuffer, int dbNumber, int byteLength)
{
    if (Cli_DBWrite(Client, dbNumber, 0, byteLength,
                    const_cast<uint8_t*>(outBuffer.data())) != 0)
        throw Snap7Exception(ERR_WRITING);
}

// ── Branje vrednosti ─────────────────────────────────────────────────────────

std::vector<uint8_t> HSnap7::ReadValuesByte(
    const std::vector<uint8_t>& data, int startAddress, int length)
{
    std::vector<uint8_t> result;
    result.reserve(length);
    for (int i = 0; i < length; ++i)
        result.push_back(data[startAddress + i]);
    return result;
}

std::vector<int16_t> HSnap7::ReadValuesInt16(
    const std::vector<uint8_t>& data, int startAddress, int length)
{
    std::vector<int16_t> result;
    result.reserve(length);
    for (int i = 0; i < length; ++i)
    {
        uint8_t buf[2];
        buf[0] = data[startAddress + i * 2];
        buf[1] = data[startAddress + i * 2 + 1];
        if (IsLittleEndian()) ReverseBytes(buf, 2);
        int16_t v;
        std::memcpy(&v, buf, 2);
        result.push_back(v);
    }
    return result;
}

std::vector<int32_t> HSnap7::ReadValuesInt32(
    const std::vector<uint8_t>& data, int startAddress, int length)
{
    std::vector<int32_t> result;
    result.reserve(length);
    for (int i = 0; i < length; ++i)
    {
        uint8_t buf[4];
        std::memcpy(buf, &data[startAddress + i * 4], 4);
        if (IsLittleEndian()) ReverseBytes(buf, 4);
        int32_t v;
        std::memcpy(&v, buf, 4);
        result.push_back(v);
    }
    return result;
}

std::vector<float> HSnap7::ReadValuesFloat(
    const std::vector<uint8_t>& data, int startAddress, int length)
{
    std::vector<float> result;
    result.reserve(length);
    for (int i = 0; i < length; ++i)
    {
        uint8_t buf[4];
        std::memcpy(buf, &data[startAddress + i * 4], 4);
        if (IsLittleEndian()) ReverseBytes(buf, 4);
        float v;
        std::memcpy(&v, buf, 4);
        result.push_back(v);
    }
    return result;
}

// ── Pisanje vrednosti ─────────────────────────────────────────────────────────

void HSnap7::WriteValuesByte(int startAddress, int length,
    std::vector<uint8_t>& outBuffer, const std::vector<DynamicValue>& values)
{
    for (int i = 0; i < length; ++i)
    {
        uint8_t v = std::get<uint8_t>(values[i]);
        outBuffer[startAddress + i] = v;
    }
}

void HSnap7::WriteValuesInt16(int startAddress, int length,
    std::vector<uint8_t>& outBuffer, const std::vector<DynamicValue>& values)
{
    for (int i = 0; i < length; ++i)
    {
        int16_t v = std::get<int16_t>(values[i]);
        uint8_t buf[2];
        std::memcpy(buf, &v, 2);
        if (IsLittleEndian()) ReverseBytes(buf, 2);
        outBuffer[startAddress + i * 2]     = buf[0];
        outBuffer[startAddress + i * 2 + 1] = buf[1];
    }
}

void HSnap7::WriteValuesInt32(int startAddress, int length,
    std::vector<uint8_t>& outBuffer, const std::vector<DynamicValue>& values)
{
    for (int i = 0; i < length; ++i)
    {
        int32_t v = std::get<int32_t>(values[i]);
        uint8_t buf[4];
        std::memcpy(buf, &v, 4);
        if (IsLittleEndian()) ReverseBytes(buf, 4);
        std::memcpy(&outBuffer[startAddress + i * 4], buf, 4);
    }
}

void HSnap7::WriteValuesFloat(int startAddress, int length,
    std::vector<uint8_t>& outBuffer, const std::vector<DynamicValue>& values)
{
    for (int i = 0; i < length; ++i)
    {
        float v = std::get<float>(values[i]);
        uint8_t buf[4];
        std::memcpy(buf, &v, 4);
        if (IsLittleEndian()) ReverseBytes(buf, 4);
        std::memcpy(&outBuffer[startAddress + i * 4], buf, 4);
    }
}

// ── Privzete vrednosti ────────────────────────────────────────────────────────

std::vector<uint8_t>  HSnap7::CreateDefaultsByte (int length) { return std::vector<uint8_t> (length, 0); }
std::vector<int16_t>  HSnap7::CreateDefaultsInt16(int length) { return std::vector<int16_t> (length, 0); }
std::vector<int32_t>  HSnap7::CreateDefaultsInt32(int length) { return std::vector<int32_t> (length, 0); }
std::vector<float>    HSnap7::CreateDefaultsFloat (int length) { return std::vector<float>   (length, 0.0f); }

} // namespace Snap7Manager
