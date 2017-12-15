# admxgen

[![Build Status](https://ci.appveyor.com/api/projects/status/github/FSLogix/admxgen?svg=true)](https://ci.appveyor.com/project/FSLogix/admxgen)

admxgen is a simple tools designed to help in generation of group policy template files. These files can be difficult to edit by hand, particularly when they become large. admxgen takes a simple json file and a csv file that can be edited in Microsoft Excel (or any text editor) and generates a properly formatted and cross-linked .admx and .adml file.

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

```
"Category","Display Name","Class","Type","Explanation","Registry Key","Value Name","Supported On","Properties"
"CheckBoxSetting","Enabled","Machine","checkBox","Description of check box setting.","Software\Company\CheckBox","CheckBoxValueName","",""
"DecimalSetting","Enabled","Machine","decimal","Description of decimal setting.","Software\Company\Decimal","DecimalValueName","","MinValue=0;MaxValue=10"
"EnumSetting","Enabled","Machine","enum","Description of enum setting.","Software\Company\Enum","EnumValueName","","Type=Decimal;Values=Option A:0|Option B:1"
"TextBoxSetting","Enabled","Machine","textBox","Description of text box setting.","Software\Company\TextBox","TextBoxValueName","",""
```
