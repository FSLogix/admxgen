# admxgen

[![Build Status](https://ci.appveyor.com/api/projects/status/github/FSLogix/admxgen?svg=true)](https://ci.appveyor.com/project/FSLogix/admxgen)

admxgen is a simple tool designed to help in generation of group policy template files. These files can be difficult to edit by hand, particularly when they become large. admxgen takes a simple json file and a csv file that can be edited in Microsoft Excel (or any text editor) and generates a properly formatted and cross-linked .admx and .adml file.

## Usage

`admxgen.exe [input_json_file] [output_filename]`

## Sample json settings file:

```
{
  "DisplayName": "Display Name",
  "Description": "Description",
  "Revision": "6.6",
  "MinRequiredRevision": "6.6",
  "SchemaVersion": "8.8",
  "TargetNamespace": {
    "Prefix": "NamespacePrefix",
    "Namespace": "NamespacePrefix.Policies"
  },
  "FallbackCulture": "en-US",
  "SupersededPolicyFiles": [
    "superseded.adm"
  ],
  "File": "checkBoxTest.csv"
}
```

## Sample csv policy definitions file:

| CategoryId | Category | Id              | Display Name      | Class   | Type            | Explanation                       | Registry Key             | Value Name        | SupportedOnId | Supported On          | Properties |
| ---        | ---      | ---             | ---               | ---     | ---             | ---                               | ---                      | ---               | ---           | ---                   | --- |
| CategoryId | Category | CheckBoxSetting | Check Box Setting | Machine | checkBox        | Description of check box setting. | Software\Company\Product | CheckBoxValueName | v1234         | Company Product v1234 | |
| CategoryId | Category | DecimalSetting  | Decimal Setting   | User    | decimal         | Description of decimal setting.   | Software\Company\Product | DecimalValueName  | v4321         | Company Product v4321 | MinValue=0;MaxValue=10;Default=5 |
| CategoryId | Category | EnabledSetting  | Enabled Setting   | Machine | enabled         | Description of enabled setting.   | Software\Company\Product | EnabledValueName  | | | |
| CategoryId | Category | EnumSetting     | Enum Setting      | User    | enum            | Description of enum setting.      | Software\Company\Product | EnumValueName     | v1234         | Company Product v1234 | Type=Decimal;Values=Option A:0\|Option B:1 |
| CategoryId | Category | TextBoxSetting  | Text Box Setting  | Machine | textBox         | Description of text box setting.  | Software\Company\Product | TextBoxValueName  | | | |
| CategoryId | Category | EnabledSetting2 | Enabled Setting 2 | Machine | enabledCheckBox | Description of setting.           | Software\Company\Product | Enabled2          | | | |
| CategoryId | Category | EnabledSetting3 | Enabled Setting 3 | Machine | enabledDropDown | Description of setting.           | Software\Company\Product | Enabled3          | | | |

NOTE: In this format you will notice several columns that represent an Id. These values are very important, and admxgen will not find all the errors that may potentially be made when editing this column. There are some rules that should be followed with these columns:

1. Ids should be only alphanumeric characters or underscores (admxgen will detect and fail).
1. The CategoryId column is unique. It is a multi-value hierarchy separated by backslashes (as is the Category column), and MUST correlate to the values in Category. (admxgen will detect if there are a different number of columns in the two fields)
1. Ids should be unique.
   1. For Id values (the policy identifier) this is self explanatory: make them unique.
   1. For SupportedOnId values, each value here will generate a new definition in the admx, so they can be reused, but make sure that they are consistent.
   1. For CategoryId, you must ensure that each category is unique across the hierarchy, so a CategoryId of 'Product\Feature' will generate a category with id Product and one with id Feature. If you do something like 'AnotherProduct\Feature' on a different entry, you are essentially going to overwrite the first Feature category with a new one.

### Supported policy types

| Policy type | Description |
| --- | --- |
| checkBox | presents a checkbox where checked sets a DWORD to 1 and unchecked sets the DWORD to 0 |
| enabledCheckBox | [alias for checkBox] |
| decimal | presents a decimal entry box, optionally specify min, max, and default values in properties |
| enabled | when enabled, creates the value, when disabled, removes the value |
| enum | presents a drop down list with values as specified in properties |
| textBox | presents a simple text entry that sets a string value |
| boolean | presents a drop down list with values Disabled and Enabled that set a DWORD to 0 and 1 respectively |
| enabledDropDown | [alias for boolean] |
