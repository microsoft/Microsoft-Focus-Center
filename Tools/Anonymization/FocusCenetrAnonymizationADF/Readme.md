---
title: Focus Center Anonymization ADF
---

# Contents {#contents .TOC-Heading}

[Introduction 2]

[Components and functionality 2]

[Pre-requisites 5]

Configuration and [Setup 5]

[Extension 6]

# Introduction

Focus Center Anonymization ADF is the Azure Data Factory that anonymize (rename/change data into non-identifiable) records for the Dataverse entities.

# Components and functionality

**Components**

Focus Center Anonymization ADF consists of:

-   Datasets (Dataverse entities)

-   Anonymization pipeline with Copy Data activities to anonymize data

-   Azure Key Vault Linked Service to access Azure Key Vault

-   Dynamics 365 Linked Service to access Dynamics 365 environment

![Graphical user interface, application Description automatically generated]

![Graphical user interface, application Description automatically generated][1]

**Functionality**

-   Anonymize entity records fields with the static values based on the FecthXML query

# Pre-requisites

-   Azure subscription to deploy Azure Data Factory.

-   App Registration (application user) to access Dynamics 365 environment. For more information, see [Create an application user].

-   Azure Key Vault secret to store App Registration secret. For more information, see [Add a secret to Key Vault].

-   Basic knowledge of Azure Data Factories to extend/modify by default functionality.

# Configuration and Setup

In order to use Focus Center Anonymization ADF the below steps need to be performed.

1.  Open FocusCenterAnonymizationADF.sln solution in the Visual Studio.

2.  Go to the ARMTemplateParametersForFactory.json and update the parameters below with your values:

    a.  **factoryName**: Data Factory name

    b.  **envServicePrincipalId**: App Registration's Client Id of application user to access Dynamics 365 environment

    c.  **keyVaultUrl**: Url to Azure Key Vault where App Registration's secret is stored.

    d.  **keyVaultSecretName**: Name of Azure Key Vault secret where App Registration's secret is stored.

    e.  **envUrl**: Dynamics 365 environment url.

    f.  **dataFactory_location**: Location to deploy Data Factory (e.g., westus).

3.  Right click on the "Pipelines", select Deploy -\> New and fill in the below details:

    a.  **Subscription:** select needed subscription from the list

    b.  **Resource group:** select needed resource group from the list

    c.  **Deployment template**: focus center anonymization\\armtemplateforfactory.json

    d.  **Template parameters file:** focus center anonymization\\armtemplateparametersforfactory.json

4.  Click "Deploy".

5.  Once Data Factory is deployed, you will see below message in Visual Studio output.\
    ![][2]

6.  By default, Data Factory does not have access to the Key Vault. Perform the steps below to provide access:

    a.  Open Data Factory in the Azure Data Factory studio:\
        ![A screenshot of a computer Description automatically generated]

    b.  Navigate to Manage -\> Linked services -\> AzureKeyVault and get Managed identity object ID:\
        ![Graphical user interface, text, application, email Description automatically generated]

    c.  Navigate to your Key Vault -\> Access policies and add new Access policy for ADF with read secrets permission.\
        \
        ![Graphical user interface, application Description automatically generated][3]

# Extension

By default, Focus Center Anonymization ADF performs anonymization of firstname, lastname and emailaddress1 attributes of Contact entity. Data Factory can be extended to anonymize different attributes/entities. In order to add a new entity to anonymize you can follow the steps below:

1.  Open Data Factory in the Azure Data Factory studio.

2.  Navigate to Author -\> Datasets and select "New Dataset".

3.  In the opened pop-up select "Dynamics 365", input you dataset name, select "FocusCenter" for Linked service and select needed entity.\
    \
    ![Graphical user interface, application, Word Description automatically generated] ![Graphical user interface, text, application, email Description automatically generated][4]

4.  Once new Dataset created, navigate to the Pipelines, clone existing pipeline and rename it.

5.  Select Copy Data activity and rename it if needed.

6.  In the Copy Data activity, go to "Source" section, update Source dataset to newly created dataset and update FetchXML query (attributes that you want to anonymize must be present in the FetchXML query).\
    ![Graphical user interface, text, application Description automatically generated]

7.  Add new values for attributes you want to anonymize as Additional columns in Source section to be used later in the mapping.\
    ![Graphical user interface, text, application, email Description automatically generated][5]

8.  Navigate to "Sink" section in the Copy Data activity and update Sink dataset with newly created dataset.

9.  Navigate to "Mapping" section in the Copy Data activity, click "Import schemas" to load new entity schema and update mapping to map anonymized attributes to entity attributes. **Do not forget to add identifier mapping so that records can be updated based on it.\
    **\
    ![Graphical user interface, application Description automatically generated][6]

10. Publish Data Factory.\
    ![][7]

  [Introduction 2]: #introduction
  [Components and functionality 2]: #_Toc89418494
  [Pre-requisites 5]: #pre-requisites
  [Setup 5]: #_Toc89418496
  [Extension 6]: #_Toc89418497
  [Graphical user interface, application Description automatically generated]: media/image1.png {width="6.5in" height="2.797222222222222in"}
  [1]: media/image2.png {width="6.5in" height="2.05625in"}
  [Create an application user]: https://docs.microsoft.com/en-us/power-platform/admin/manage-application-users#create-an-application-user
  [Add a secret to Key Vault]: https://docs.microsoft.com/en-us/azure/key-vault/secrets/quick-create-portal#add-a-secret-to-key-vault
  [2]: media/image3.png {width="6.5in" height="0.24305555555555555in"}
  [A screenshot of a computer Description automatically generated]: media/image4.png {width="5.831746500437445in" height="2.214319772528434in"}
  [Graphical user interface, text, application, email Description automatically generated]: media/image5.png {width="5.7369149168853895in" height="2.458415354330709in"}
  [3]: media/image6.png {width="5.666189851268592in" height="2.236207349081365in"}
  [Graphical user interface, application, Word Description automatically generated]: media/image7.png {width="2.2588331146106735in" height="3.1145833333333335in"}
  [4]: media/image8.png {width="2.6584700349956254in" height="3.1288123359580053in"}
  [Graphical user interface, text, application Description automatically generated]: media/image9.png {width="6.5in" height="2.8958333333333335in"}
  [5]: media/image10.png {width="6.5in" height="2.792361111111111in"}
  [6]: media/image11.png {width="6.5in" height="2.5368055555555555in"}
  [7]: media/image12.png {width="6.5in" height="2.7875in"}
