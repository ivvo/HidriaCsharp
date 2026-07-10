#pragma once

namespace Snap7Manager
{
    /// @brief Enumeracija predstavlja stanje povezave s PLC-jem.
    enum class PLCConnectionStatus
    {
        Offline,
        Online,
        Error
    };

} // namespace Snap7Manager
