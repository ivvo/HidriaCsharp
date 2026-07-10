#pragma once

#include <string>
#include <vector>
#include <map>
#include <variant>
#include <cstdint>
#include "Other.h"
#include "Exceptions.h"

// Predpostavimo, da je snap7.h na voljo (nativna C knjižnica Snap7)
#include "snap7.h"

namespace Snap7Manager
{
    /// @brief Tip za dinamično vrednost (Byte, Int16, Int32, Float).
    using DynamicValue = std::variant<uint8_t, int16_t, int32_t, float>;

    /// @brief Razred HSnap7 – ovoj za komunikacijo s PLC-jem prek knjižnice Snap7.
    class HSnap7
    {
    public:
        HSnap7();
        ~HSnap7() { if (Client) Cli_Destroy(&Client); }

        // ── Javne metode ────────────────────────────────────────────────────────

        /// @brief Vzpostavi povezavo s PLC-jem.
        /// @param address IP naslov PLC-ja.
        /// @param rack    Rack PLC-ja.
        /// @param slot    Slot PLC-ja.
        /// @throws Snap7Exception ob napaki pri vzpostavljanju povezave.
        void PlcConnect(const std::string& address, int rack, int slot);

        /// @brief Prekine povezavo s PLC-jem.
        void PlcDisconnect();

        /// @brief Prebere sekcije DB bloka in vrne podatke.
        /// @param dbSections Opis sekcij DB bloka.
        /// @return Slovar z imeni (»ime:tip«) in seznami vrednosti.
        /// @throws Snap7Exception ob napaki pri branju.
        std::map<std::string, std::vector<DynamicValue>>
            GetSections(const DBSection& dbSections);

        /// @brief Zapiše podatke v sekcije DB bloka.
        /// @param valuesAndNames Slovar z imeni in vrednostmi.
        /// @param dbSections     Opis sekcij DB bloka.
        /// @throws Snap7Exception ob napaki pri pisanju.
        void SetSections(
            const std::map<std::string, std::vector<DynamicValue>>& valuesAndNames,
            const DBSection& dbSections);

        /// @brief Ustvari privzeti slovar z ničelnimi vrednostmi.
        /// @param dbSections Opis sekcij DB bloka.
        /// @return Slovar z privzetimi vrednostmi.
        std::map<std::string, std::vector<DynamicValue>>
            CreateDefaultDictionary(const DBSection& dbSections);

    private:
        // ── Konstante ────────────────────────────────────────────────────────────
        static constexpr const char* ERR_CONNECTION  = "Error Connection";
        static constexpr const char* ERR_READING     = "Error when Reading";
        static constexpr const char* ERR_WRITING     = "Error when Writing";
        static constexpr const char* ERR_CONNECTING  = "Error Connecting to PLC";

        // ── Zasebna polja ────────────────────────────────────────────────────────
        S7Object Client;   ///< Handle Snap7 odjemalca
        std::map<std::string, std::vector<DynamicValue>> SectionsData;

        // ── Pomožne metode ───────────────────────────────────────────────────────

        /// @brief Izračuna skupno dolžino v bajtih glede na sekcije DB bloka.
        int ComputeByteLength(const DBSection& dbSections) const;

        /// @brief Prebere DB blok v polje bajtov.
        std::vector<uint8_t> ReadPLC(const DBSection& dbSections);

        /// @brief Zapiše polje bajtov v DB blok.
        void WritePLC(const std::vector<uint8_t>& outBuffer, int dbNumber, int byteLength);

        // Generični pomočniki za branje vrednosti iz polja bajtov
        std::vector<uint8_t>  ReadValuesByte (const std::vector<uint8_t>& data, int startAddress, int length);
        std::vector<int16_t>  ReadValuesInt16(const std::vector<uint8_t>& data, int startAddress, int length);
        std::vector<int32_t>  ReadValuesInt32(const std::vector<uint8_t>& data, int startAddress, int length);
        std::vector<float>    ReadValuesFloat (const std::vector<uint8_t>& data, int startAddress, int length);

        // Generični pomočniki za pisanje vrednosti v polje bajtov
        void WriteValuesByte (int startAddress, int length, std::vector<uint8_t>& outBuffer,
                              const std::vector<DynamicValue>& values);
        void WriteValuesInt16(int startAddress, int length, std::vector<uint8_t>& outBuffer,
                              const std::vector<DynamicValue>& values);
        void WriteValuesInt32(int startAddress, int length, std::vector<uint8_t>& outBuffer,
                              const std::vector<DynamicValue>& values);
        void WriteValuesFloat (int startAddress, int length, std::vector<uint8_t>& outBuffer,
                              const std::vector<DynamicValue>& values);

        // Pomočniki za ustvarjanje privzetih vrednosti
        std::vector<uint8_t>  CreateDefaultsByte (int length);
        std::vector<int16_t>  CreateDefaultsInt16(int length);
        std::vector<int32_t>  CreateDefaultsInt32(int length);
        std::vector<float>    CreateDefaultsFloat (int length);

        /// @brief Vrne true, če sistem uporablja little-endian zapis.
        static bool IsLittleEndian();

        /// @brief Obrne vrstni red bajtov v bloku.
        static void ReverseBytes(uint8_t* bytes, int count);
    };

} // namespace Snap7Manager
