<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FZaid-Safadi%2FDICOMcloud%2FDevelopment%2Fazuredeploy.json">
	<img src="https://camo.githubusercontent.com/9285dd3998997a0835869065bb15e5d500475034/687474703a2f2f617a7572656465706c6f792e6e65742f6465706c6f79627574746f6e2e706e67" data-canonical-src="http://azuredeploy.net/deploybutton.png" style="max-width:100%;">
</a> 
<a href="http://armviz.io/#/?load=https%3A%2F%2Fraw.githubusercontent.com%2FZaid-Safadi%2FDICOMcloud%2FDevelopment%2Fazuredeploy.json">
    <img src="https://camo.githubusercontent.com/536ab4f9bc823c2e0ce72fb610aafda57d8c6c12/687474703a2f2f61726d76697a2e696f2f76697375616c697a65627574746f6e2e706e67" data-canonical-src="http://armviz.io/visualizebutton.png" style="max-width:100%;">
</a>

# DICOMcloud

[![Join the chat at https://gitter.im/Zaid-Safadi/DICOMcloud](https://badges.gitter.im/Zaid-Safadi/DICOMcloud.svg)](https://gitter.im/Zaid-Safadi/DICOMcloud?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)  [![Build status](https://ci.appveyor.com/api/projects/status/v9c3lcjv9xymabww/branch/master?svg=true)](https://ci.appveyor.com/project/Zaid-Safadi/dicomcloud/branch/development)

DICOMcloud is a highly customizable, open source DICOMweb server that implements RESTful services in DICOM part 18

The DICOM Web server implementation uses Unity as a Dependency Injection (DI) framework to build the server components. You can extend any of these components or completely roll out your own then plug it in using your DI framework of choice. The implementation takes advantage of this architecture by providing two options for media storage: 
 1. Local File System: A module that support storing and retrieving media files to a local file system storage using the Windows File API in .NET 
 2. Azure Blob Storage: A module that uses the Azure Storage library for storing DICOM files and any other media to Azure Blob storage.

You can do the same with the database implementation for example to replace the existing implementation or customize it to integrate with your own database and traditional DICOM server. 

# Hosted server endpoints and demo
I'm maintining an online version of the server, check the wiki [Home](https://github.com/Zaid-Safadi/DICOMcloud/wiki) page for endpoint Urls 

There is a client Demo that I open sourced and hosting:
[https://github.com/Zaid-Safadi/dicom-webJS](https://github.com/Zaid-Safadi/dicom-webJS/)

# Implementation
The server code is written in C# .NET 4.5.2 and Visual Studio 2015. The web services are built as ASP.NET REST WebApi Controllers.
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
