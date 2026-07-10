#pragma once

#include <string>
#include <vector>
#include <typeindex>
#include "Enums.h"

namespace Snap7Manager
{
    /// @brief Notranja struktura podatkov DB bloka.
    struct DBData
    {
        std::string  Name;
        std::type_index DataType;
        int          StartAddress;
        int          Length;

        DBData()
            : DataType(typeid(void)), StartAddress(0), Length(0) {}

        DBData(const std::string& name, std::type_index dataType, int startAddress, int length)
            : Name(name), DataType(dataType), StartAddress(startAddress), Length(length) {}
    };

    /// @brief Razred predstavlja sekcijo DB bloka.
    class DBSection
    {
    public:
        int              DBNumber;
        std::vector<DBData> Data;

        DBSection() : DBNumber(0) {}
    };

    /// @brief Razred predstavlja argumente dogodka za spremembo stanja povezave.
    class Snap7ConnectionStatusChangedEventArgs
    {
    public:
        /// @brief Stanje povezave s PLC-jem.
        PLCConnectionStatus ConnectionStatus;

        /// @brief Ustvari nov primerek razreda.
        /// @param connectionStatus Stanje povezave s PLC-jem.
        explicit Snap7ConnectionStatusChangedEventArgs(PLCConnectionStatus connectionStatus)
            : ConnectionStatus(connectionStatus) {}
    };

} // namespace Snap7Manager
