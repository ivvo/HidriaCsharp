#pragma once

// Zahteva knjižnico pugixml (https://pugixml.org/)
// ali tinyxml2 (https://github.com/leethomason/tinyxml2).
// Ta implementacija uporablja pugixml.
#include "../XMLSource/pugixml.hpp"

#include <string>
#include <vector>
#include <map>
#include <mutex>
#include <variant>
#include <stdexcept>

namespace XMLSettings
{
    /// @brief Tip za vrednost XML elementa (int, double, bool, string).
    using XMLValue = std::variant<int, double, bool, std::string>;

    /// @brief Razred za upravljanje XML nastavitev.
    class XMLSettingsManager
    {
    public:
        /// @brief Konstruktor – naloži XML datoteko.
        /// @param folderPath Pot do mape z datoteko.
        /// @param fileName   Ime datoteke.
        /// @throws std::invalid_argument  Če je pot ali ime prazno.
        /// @throws std::runtime_error     Če datoteka ne obstaja ali je napačna.
        XMLSettingsManager(const std::string& folderPath, const std::string& fileName);

        // ── Javne metode ─────────────────────────────────────────────────────────

        /// @brief Vrne vrednost določenega elementa.
        /// @param segmentName      Ime segmenta.
        /// @param segmentAttribute Atribut segmenta (Label).
        /// @param name             Ime elementa.
        /// @return Vrednost elementa.
        XMLValue GetElement(const std::string& segmentName,
                            const std::string& segmentAttribute,
                            const std::string& name);

        /// @brief Nastavi vrednost določenega elementa in shrani datoteko.
        /// @param segmentName      Ime segmenta.
        /// @param segmentAttribute Atribut segmenta (Label).
        /// @param name             Ime elementa.
        /// @param value            Nova vrednost.
        void SetElement(const std::string& segmentName,
                        const std::string& segmentAttribute,
                        const std::string& name,
                        const XMLValue& value);

        /// @brief Vrne vse vrednosti določenega segmenta kot slovar.
        /// @param segmentName      Ime segmenta.
        /// @param segmentAttribute Atribut segmenta (Label).
        /// @return Slovar ime → vrednost.
        std::map<std::string, XMLValue> GetSegment(const std::string& segmentName,
                                                   const std::string& segmentAttribute);

        /// @brief Nastavi vse vrednosti določenega segmenta in shrani datoteko.
        /// @param segmentName      Ime segmenta.
        /// @param segmentAttribute Atribut segmenta (Label).
        /// @param segment          Slovar ime → vrednost.
        void SetSegment(const std::string& segmentName,
                        const std::string& segmentAttribute,
                        const std::map<std::string, XMLValue>& segment);

        /// @brief Vrne seznam vseh segmentov z danim imenom in atributom.
        /// @param segmentName      Ime nadrejenega segmenta.
        /// @param segmentAttribute Atribut nadrejenega segmenta (Label).
        /// @param name             Ime podsegmentov.
        /// @return Seznam slovarjev ime → vrednost.
        std::vector<std::map<std::string, XMLValue>>
            GetSegments(const std::string& segmentName,
                        const std::string& segmentAttribute,
                        const std::string& name);

    private:
        // ── Zasebna polja ────────────────────────────────────────────────────────
        pugi::xml_document XMLDoc;
        pugi::xml_node     XMLRoot;
        std::string        XMLPath;
        std::string        FileName;
        std::mutex         Mtx;

        // ── Pomožne metode ───────────────────────────────────────────────────────

        /// @brief Poišče elemente znotraj segmenta.
        std::vector<pugi::xml_node> GetSegmentElements(const std::string& segmentName,
                                                       const std::string& segmentAttribute);

        /// @brief Poišče posamičen element znotraj segmenta.
        pugi::xml_node GetSegmentElement(const std::vector<pugi::xml_node>& segmentElements,
                                         const std::string& name);

        /// @brief Razčleni vrednost XML elementa glede na atribut »Type«.
        XMLValue ParseElementValue(const pugi::xml_node& node);

        /// @brief Pretvori XMLValue v niz za shranjevanje.
        std::string ValueToString(const XMLValue& value);

        /// @brief Shrani XML datoteko na disk.
        void SaveXML();
    };

} // namespace XMLSettings
