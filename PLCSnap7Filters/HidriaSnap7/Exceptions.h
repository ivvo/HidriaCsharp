#pragma once

#include <stdexcept>
#include <string>

namespace Snap7Manager
{
    /// @brief Razred predstavlja izjemo za Snap7.
    class Snap7Exception : public std::runtime_error
    {
    public:
        /// @brief Ustvari izjemo brez sporočila.
        Snap7Exception()
            : std::runtime_error("Snap7Exception") {}

        /// @brief Ustvari izjemo s sporočilom.
        /// @param message Sporočilo izjeme.
        explicit Snap7Exception(const std::string& message)
            : std::runtime_error(message) {}
    };

} // namespace Snap7Manager
