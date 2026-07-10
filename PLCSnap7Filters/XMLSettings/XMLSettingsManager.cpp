#include "XMLSettingsManager.h"
#include <filesystem>
#include <sstream>
#include <algorithm>

#include "../XMLSource/pugixml.hpp"

namespace fs = std::filesystem;

namespace XMLSettings
{

// ── Konstruktor ───────────────────────────────────────────────────────────────

XMLSettingsManager::XMLSettingsManager(const std::string& folderPath,
                                       const std::string& fileName)
{
    if (folderPath.empty())
        throw std::invalid_argument("Folder path is null or empty.");
    if (fileName.empty())
        throw std::invalid_argument("File name is null or empty.");

    XMLPath  = folderPath;
    FileName = fileName;

    fs::path fullPath = fs::path(folderPath) / fileName;
    pugi::xml_parse_result result = XMLDoc.load_file(fullPath.string().c_str());

    if (!result)
        throw std::runtime_error(std::string("XML parse error: ") + result.description());

    XMLRoot = XMLDoc.document_element();
}

// ── Javne metode ──────────────────────────────────────────────────────────────

XMLValue XMLSettingsManager::GetElement(const std::string& segmentName,
                                        const std::string& segmentAttribute,
                                        const std::string& name)
{
    if (segmentName.empty())
        throw std::invalid_argument("Segment name is null or empty.");
    if (segmentAttribute.empty())
        throw std::invalid_argument("Segment attribute is null or empty.");
    if (name.empty())
        throw std::invalid_argument("Name is null or empty.");

    std::lock_guard<std::mutex> lock(Mtx);

    auto elements = GetSegmentElements(segmentName, segmentAttribute);

    long count = std::count_if(elements.begin(), elements.end(),
        [&](const pugi::xml_node& n){ return std::string(n.name()) == name; });

    if (count != 1)
        throw std::runtime_error("There are no elements or there are multiple elements with the same name.");

    pugi::xml_node element = GetSegmentElement(elements, name);

    if (!element.attribute("Type"))
        throw std::runtime_error("Type not specified.");

    return ParseElementValue(element);
}

void XMLSettingsManager::SetElement(const std::string& segmentName,
                                    const std::string& segmentAttribute,
                                    const std::string& name,
                                    const XMLValue& value)
{
    if (segmentName.empty())
        throw std::invalid_argument("Segment name is null or empty.");
    if (segmentAttribute.empty())
        throw std::invalid_argument("Segment attribute is null or empty.");
    if (name.empty())
        throw std::invalid_argument("Name is null or empty.");

    std::lock_guard<std::mutex> lock(Mtx);

    auto elements = GetSegmentElements(segmentName, segmentAttribute);

    long count = std::count_if(elements.begin(), elements.end(),
        [&](const pugi::xml_node& n){ return std::string(n.name()) == name; });

    if (count != 1)
        throw std::runtime_error("There are no elements or there are multiple elements with the same name.");

    pugi::xml_node element = GetSegmentElement(elements, name);

    if (!element.attribute("Type"))
        throw std::runtime_error("Type not specified.");

    element.text().set(ValueToString(value).c_str());
    SaveXML();
}

std::map<std::string, XMLValue>
XMLSettingsManager::GetSegment(const std::string& segmentName,
                               const std::string& segmentAttribute)
{
    if (segmentName.empty())
        throw std::invalid_argument("Segment name is null or empty.");
    if (segmentAttribute.empty())
        throw std::invalid_argument("Segment attribute is null or empty.");

    std::lock_guard<std::mutex> lock(Mtx);

    auto elements = GetSegmentElements(segmentName, segmentAttribute);
    std::map<std::string, XMLValue> result;

    for (const pugi::xml_node& el : elements)
    {
        if (!el.attribute("Type"))
            throw std::runtime_error("Type not specified.");
        result[el.name()] = ParseElementValue(el);
    }

    return result;
}

void XMLSettingsManager::SetSegment(const std::string& segmentName,
                                    const std::string& segmentAttribute,
                                    const std::map<std::string, XMLValue>& segment)
{
    if (segmentName.empty())
        throw std::invalid_argument("Segment name is null or empty.");
    if (segmentAttribute.empty())
        throw std::invalid_argument("Segment attribute is null or empty.");

    std::lock_guard<std::mutex> lock(Mtx);

    auto elements = GetSegmentElements(segmentName, segmentAttribute);

    for (const auto& [key, val] : segment)
    {
        long count = std::count_if(elements.begin(), elements.end(),
            [&](const pugi::xml_node& n){ return std::string(n.name()) == key; });

        if (count != 1)
            throw std::runtime_error("There are no elements or there are multiple elements with the same name.");

        pugi::xml_node element = GetSegmentElement(elements, key);

        if (!element.attribute("Type"))
            throw std::runtime_error("Type not specified.");

        element.text().set(ValueToString(val).c_str());
    }

    SaveXML();
}

std::vector<std::map<std::string, XMLValue>>
XMLSettingsManager::GetSegments(const std::string& segmentName,
                                const std::string& segmentAttribute,
                                const std::string& name)
{
    if (segmentName.empty())
        throw std::invalid_argument("Segment name is null or empty.");
    if (segmentAttribute.empty())
        throw std::invalid_argument("Segment attribute is null or empty.");
    if (name.empty())
        throw std::invalid_argument("Name is null or empty.");

    std::lock_guard<std::mutex> lock(Mtx);

    auto mainElements = GetSegmentElements(segmentName, segmentAttribute);
    std::vector<std::map<std::string, XMLValue>> result;

    for (const pugi::xml_node& mainEl : mainElements)
    {
        if (std::string(mainEl.name()) != name)
            continue;

        std::map<std::string, XMLValue> segmentMap;

        for (pugi::xml_node child : mainEl.children())
        {
            // Preveri, da element nima podelementov
            if (child.first_child().type() == pugi::node_element)
                throw std::runtime_error("Cannot get the element which has other elements.");

            if (!child.attribute("Type"))
                throw std::runtime_error("Type not specified.");

            segmentMap[child.name()] = ParseElementValue(child);
        }

        result.push_back(std::move(segmentMap));
    }

    return result;
}

// ── Zasebne metode ────────────────────────────────────────────────────────────

std::vector<pugi::xml_node>
XMLSettingsManager::GetSegmentElements(const std::string& segmentName,
                                       const std::string& segmentAttribute)
{
    std::vector<pugi::xml_node> segments;

    for (pugi::xml_node child : XMLRoot.children(segmentName.c_str()))
    {
        pugi::xml_attribute labelAttr = child.attribute("Label");
        if (labelAttr && std::string(labelAttr.value()) == segmentAttribute)
            segments.push_back(child);
    }

    if (segments.size() != 1)
        throw std::runtime_error(
            "There is no segment or there are multiple segments with the same name and attribute.");

    std::vector<pugi::xml_node> elements;
    for (pugi::xml_node el : segments[0].children())
        elements.push_back(el);

    if (elements.empty())
        throw std::runtime_error("Cannot get the segment which does not have elements.");

    return elements;
}

pugi::xml_node
XMLSettingsManager::GetSegmentElement(const std::vector<pugi::xml_node>& segmentElements,
                                      const std::string& name)
{
    for (const pugi::xml_node& el : segmentElements)
    {
        if (std::string(el.name()) == name)
        {
            // Element ne sme imeti podelementov
            if (el.first_child().type() == pugi::node_element)
                throw std::runtime_error("Cannot get the element which has other elements.");
            return el;
        }
    }
    throw std::runtime_error("Element not found: " + name);
}

XMLValue XMLSettingsManager::ParseElementValue(const pugi::xml_node& node)
{
    std::string typeStr = node.attribute("Type").value();
    std::string valStr  = node.text().as_string();

    if (typeStr == "int" || typeStr == "System.Int32")
        return std::stoi(valStr);
    else if (typeStr == "double" || typeStr == "System.Double")
        return std::stod(valStr);
    else if (typeStr == "bool" || typeStr == "System.Boolean")
        return valStr == "true" || valStr == "True" || valStr == "1";
    else // string ali System.String
        return valStr;
}

std::string XMLSettingsManager::ValueToString(const XMLValue& value)
{
    return std::visit([](const auto& v) -> std::string {
        if constexpr (std::is_same_v<std::decay_t<decltype(v)>, bool>)
            return v ? "true" : "false";
        else if constexpr (std::is_same_v<std::decay_t<decltype(v)>, std::string>)
            return v;
        else
            return std::to_string(v);
    }, value);
}

void XMLSettingsManager::SaveXML()
{
    fs::path fullPath = fs::path(XMLPath) / FileName;
    if (!XMLDoc.save_file(fullPath.string().c_str()))
        throw std::runtime_error("Failed to save XML file: " + fullPath.string());
}

} // namespace XMLSettings
