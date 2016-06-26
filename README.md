# DICOMcloud

[![Join the chat at https://gitter.im/Zaid-Safadi/DICOMcloud](https://badges.gitter.im/Zaid-Safadi/DICOMcloud.svg)](https://gitter.im/Zaid-Safadi/DICOMcloud?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)  [![Build status](https://ci.appveyor.com/api/projects/status/v9c3lcjv9xymabww/branch/master?svg=true)](https://ci.appveyor.com/project/Zaid-Safadi/dicomcloud/branch/fo-dicom-integration)

DICOMcloud is an open source DICOMweb server that implements RESTful services in DICOM part 18


# Hosted server endpoints and demo
I'm maintining an online version of the server, check the wiki [Home](https://github.com/Zaid-Safadi/DICOMcloud/wiki) page for endpoint Urls 

There is a client Demo that I open sourced and hosting:
[https://github.com/Zaid-Safadi/dicom-webJS](https://github.com/Zaid-Safadi/dicom-webJS/)

# Implementation
The code is built with .NET 4.5.2 and Visual Studio 2015. The web services are built as ASP.NET REST WebApi Controllers.
Check the [code and project's structure](https://github.com/Zaid-Safadi/DICOMcloud/wiki/Code-and-Projects-Structure) wiki page for more details.

Physical DICOM storage is supported on both, either Windows File System or Azure Blob Storage.

Query is currently implemented against a SQL database and compatible with Azure SQL Database

Implementation natively support JSON and XML DICOM format.

# DICOM Support
The code is designed to be a complete DICOM web server implementation with storage, query and retrieve capabilities.

For detailed information about the supported services and features, check the [DICOM Support](https://github.com/Zaid-Safadi/DICOMcloud/wiki/DICOM-Support) wiki page  

# License
 
    Copyright 2016 Zaid AL-Safadi

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
