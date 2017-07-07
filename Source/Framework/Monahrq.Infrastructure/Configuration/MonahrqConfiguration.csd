<?xml version="1.0" encoding="utf-8"?>
<configurationSectionModel xmlns:dm0="http://schemas.microsoft.com/VisualStudio/2008/DslTools/Core" dslVersion="1.0.0.0" Id="f898a38c-21e8-453f-84df-9cbcb660030e" namespace="Monahrq.Infrastructure.Configuration" xmlSchemaNamespace="urn:Monahrq.Infrastructure.Configuration" xmlns="http://schemas.microsoft.com/dsltools/ConfigurationSectionDesigner">
  <typeDefinitions>
    <externalType name="String" namespace="System" />
    <externalType name="Boolean" namespace="System" />
    <externalType name="Int32" namespace="System" />
    <externalType name="Int64" namespace="System" />
    <externalType name="Single" namespace="System" />
    <externalType name="Double" namespace="System" />
    <externalType name="DateTime" namespace="System" />
    <externalType name="TimeSpan" namespace="System" />
    <externalType name="ConnectionStringSettings" namespace="System.Configuration" />
    <externalType name="StringCollection" namespace="System.Collections.Specialized" />
  </typeDefinitions>
  <configurationElements>
    <configurationSectionGroup name="MonahrqConfigurationSectionGroup">
      <configurationSectionProperties>
        <configurationSectionProperty>
          <containedConfigurationSection>
            <configurationSectionMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/MonahrqConfigurationSection" />
          </containedConfigurationSection>
        </configurationSectionProperty>
      </configurationSectionProperties>
    </configurationSectionGroup>
    <configurationSection name="MonahrqConfigurationSection" codeGenOptions="Singleton, XmlnsProperty" xmlSectionName="MonahrqConfigurationSection">
      <attributeProperties>
        <attributeProperty name="RebuildDatabase" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="rebuildDatabase" isReadOnly="false" defaultValue="false">
          <customAttributes>
            <attribute name="System.Configuration.ApplicationScopedSettingAttribute" />
          </customAttributes>
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/Boolean" />
          </type>
        </attributeProperty>
        <attributeProperty name="LongTimeout" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="longTimeout" isReadOnly="false" defaultValue="&quot;00:05:00&quot;">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/TimeSpan" />
          </type>
        </attributeProperty>
        <attributeProperty name="ShortTimeout" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="shortTimeout" isReadOnly="false" defaultValue="&quot;00:00:30&quot;">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/TimeSpan" />
          </type>
        </attributeProperty>
        <attributeProperty name="LastFolder" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="lastFolder" isReadOnly="false" defaultValue="&quot;&quot;">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="BatchSize" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="batchSize" isReadOnly="false" defaultValue="1000">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="DebugSql" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="debugSql" isReadOnly="false" defaultValue="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/Boolean" />
          </type>
        </attributeProperty>
        <attributeProperty name="UpdateCheckUrl" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="updateCheckUrl" isReadOnly="false" defaultValue="&quot;http://www.ahrq.gov/professionals/systems/monahrq/updates/update.xml&quot;">
          <customAttributes>
            <attribute name="System.Configuration.ApplicationScopedSettingAttribute" />
          </customAttributes>
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="UseApiForPhysicians" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="useApiForPhysicians" isReadOnly="false" defaultValue="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/Boolean" />
          </type>
        </attributeProperty>
        <attributeProperty name="DataAccessComponentsInstalled" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="dataAccessComponentsInstalled" isReadOnly="false" defaultValue="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/Boolean" />
          </type>
        </attributeProperty>
        <attributeProperty name="MonahrqVersion" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="monahrqVersion" isReadOnly="false">
          <customAttributes>
            <attribute name="System.Configuration.ApplicationScopedSettingAttribute" />
          </customAttributes>
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="MonahrqDemoSiteUrl" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="monahrqDemoSiteUrl" isReadOnly="false" defaultValue="&quot;http://www.ahrq.gov/professionals/systems/monahrq/demo/MONAHRQ60b2/index.html#&quot;">
          <customAttributes>
            <attribute name="System.Configuration.ApplicationScopedSettingAttribute" />
          </customAttributes>
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
      <elementProperties>
        <elementProperty name="NamedConnections" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="namedConnections" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/NamedConnectionElementCollection" />
          </type>
        </elementProperty>
        <elementProperty name="ContentTypes" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="contentTypes" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/MonahrqContentTypeElementCollection" />
          </type>
        </elementProperty>
        <elementProperty name="EntityConnectionSettings" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="entityConnectionSettings" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/ConnectionStringSettingsElement" />
          </type>
        </elementProperty>
        <elementProperty name="Themes" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="themes" isReadOnly="false">
          <customAttributes>
            <attribute name="System.Configuration.ApplicationScopedSettingAttribute" />
          </customAttributes>
          <type>
            <configurationElementCollectionMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/MonahrqThemeElementCollection" />
          </type>
        </elementProperty>
        <elementProperty name="HospitalRegion" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="hospitalRegion" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/HospitalRegionElement" />
          </type>
        </elementProperty>
        <elementProperty name="WinQiConnectionSettings" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="winQiConnectionSettings" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/ConnectionStringSettingsElement" />
          </type>
        </elementProperty>
        <elementProperty name="Banners" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="banners" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/MonahrqBannerElementCollection" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationSection>
    <configurationElement name="NamedConnectionElement">
      <attributeProperties>
        <attributeProperty name="Name" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="name" isReadOnly="false" displayName="Connection Name">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="ConnectionString" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="connectionString" isReadOnly="false" displayName="Connection String">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="SelectFrom" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="selectFrom" isReadOnly="false" documentation="The object from the datasource to present to the application" displayName="Select From">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="ControllerType" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="controllerType" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
      <elementProperties>
        <elementProperty name="SchemaIniSettings" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="schemaIniSettings" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/SchemaIniElementCollection" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationElement>
    <configurationElementCollection name="NamedConnectionElementCollection" xmlItemName="namedConnectionElement" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/NamedConnectionElement" />
      </itemType>
    </configurationElementCollection>
    <configurationElement name="MonahrqSettingElement">
      <attributeProperties>
        <attributeProperty name="Name" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="name" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Value" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="value" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElementCollection name="MonahrqSettingElementCollection" xmlItemName="monahrqSettingElement" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/MonahrqSettingElement" />
      </itemType>
    </configurationElementCollection>
    <configurationElement name="MonahrqNamedSettingsElement">
      <attributeProperties>
        <attributeProperty name="Name" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="name" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
      <elementProperties>
        <elementProperty name="Settings" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="settings" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/MonahrqSettingElementCollection" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationElement>
    <configurationElement name="MonahrqContentPartElement">
      <baseClass>
        <configurationElementMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/MonahrqNamedSettingsElement" />
      </baseClass>
    </configurationElement>
    <configurationElementCollection name="MonahrqContentPartElementCollection" xmlItemName="monahrqContentPartElement" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/MonahrqContentPartElement" />
      </itemType>
    </configurationElementCollection>
    <configurationElement name="MonahrqContentTypeElement">
      <baseClass>
        <configurationElementMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/MonahrqNamedSettingsElement" />
      </baseClass>
      <elementProperties>
        <elementProperty name="ContentParts" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="contentParts" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/MonahrqContentPartElementCollection" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationElement>
    <configurationElementCollection name="MonahrqContentTypeElementCollection" xmlItemName="monahrqContentTypeElement" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/MonahrqContentTypeElement" />
      </itemType>
    </configurationElementCollection>
    <configurationElement name="SchemaIniElement">
      <attributeProperties>
        <attributeProperty name="Name" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="name" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Value" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="value" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElementCollection name="SchemaIniElementCollection" xmlItemName="schemaIniElement" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/SchemaIniElement" />
      </itemType>
    </configurationElementCollection>
    <configurationElement name="ConnectionStringSettingsElement">
      <attributeProperties>
        <attributeProperty name="ProviderName" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="providerName" isReadOnly="false" defaultValue="&quot;System.Data.SqlClient&quot;">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="ConnectionString" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="connectionString" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElement name="MonahrqThemeElement">
      <attributeProperties>
        <attributeProperty name="Name" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="name" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="IsDefault" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="isDefault" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/Boolean" />
          </type>
        </attributeProperty>
        <attributeProperty name="BrandColor" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="brandColor" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="AccentColor" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="accentColor" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="BackgroundColor" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="backgroundColor" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Accent2Color" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="accent2Color" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="BodyTextColor" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="bodyTextColor" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="LinkTextColor" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="linkTextColor" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Brand2Color" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="brand2Color" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElementCollection name="MonahrqThemeElementCollection" xmlItemName="theme" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods" addItemName="monahrqThemeElement" removeItemName="monahrqThemeElement" clearItemsName="monahrqThemeElement">
      <attributeProperties>
        <attributeProperty name="JsonFileExtension" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="json-file-extension" isReadOnly="false" defaultValue="&quot;json&quot;">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
      <itemType>
        <configurationElementMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/MonahrqThemeElement" />
      </itemType>
    </configurationElementCollection>
    <configurationElement name="HospitalRegionElement">
      <attributeProperties>
        <attributeProperty name="DefaultRegionTypeName" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="defaultRegionTypeName" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="DefaultStatesBAD" isRequired="false" isKey="false" isDefaultCollection="true" xmlName="defaultStatesBAD" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/StringCollection" />
          </type>
        </attributeProperty>
      </attributeProperties>
      <elementProperties>
        <elementProperty name="DefaultStatesProxy" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="defaultStatesProxy" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/StatesCollectionElement" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationElement>
    <configurationElement name="ConnectionStringSettingsE">
      <attributeProperties>
        <attributeProperty name="ConnectionStringSettings" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="connectionStringSettings" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/ConnectionStringSettings" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElementCollection name="StatesCollectionElement" xmlItemName="stateElement" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/StateElement" />
      </itemType>
    </configurationElementCollection>
    <configurationElement name="StateElement">
      <attributeProperties>
        <attributeProperty name="StateName" isRequired="true" isKey="true" isDefaultCollection="true" xmlName="stateName" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElementCollection name="MonahrqBannerElementCollection" xmlItemName="banner" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/MonahrqBannerElement" />
      </itemType>
    </configurationElementCollection>
    <configurationElement name="MonahrqBannerElement">
      <attributeProperties>
        <attributeProperty name="Name" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="name" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Value" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="value" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
  </configurationElements>
  <propertyValidators>
    <validators>
      <integerValidator name="BatchSizeValidator" excludeRange="true" maxValue="65536" minValue="1" />
    </validators>
  </propertyValidators>
  <customTypeConverters>
    <converter name="TimespanFromStringConverter">
      <type>
        <externalTypeMoniker name="/f898a38c-21e8-453f-84df-9cbcb660030e/TimeSpan" />
      </type>
    </converter>
  </customTypeConverters>
</configurationSectionModel>