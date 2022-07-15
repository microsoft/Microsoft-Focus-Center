---
title: Focus Center PR Checker Tool
---

# Contents {#contents .TOC-Heading}

[Introduction 2]

[Components and functionality 2]

[Pre-requisites 5]

[Setup 5]

[Configuration 6]

# Introduction

Every development team reviews PR to check key attributes to ensure that the PR is adhering to standards. Some of the checks are routine and important. Automating routine checks will improve efficiency and improve quality of builds.

PR checker tool is one such tool which will automate the routine checks and post comments on PR. Validating solution publishers and solution versions are a few automated checks in the tool.

# Components and functionality

## Components

PR Checker Tool consist of four components:

1.  tool's exe,

2.  tool's app config,

3.  pipeline variable group

4.  command line script.

![Graphical user interface, text, application, email Description automatically generated]

![Table Description automatically generated]

Users will see review comments from PR checker tool on their respective PRs.

![Graphical user interface, text, application, email Description automatically generated][1]

## Functionality

The PR checker currently has the following features:

1.  **[Check duplicate components]{.underline}**: Checks if a component is present across multiple solutions in the repo. This way a particular component is not customized in multiple solutions by mistake. It will also ensure clear ownership of components in a solution and prevent overwriting of customizations in Test/Prod environments.

2.  **[Check duplicate connection references]{.underline}**: Check if any connection references is present across multiple solutions in the repo.

3.  **Check entity-level business rules:** Check if any new/modified entity-level business rules are present in PR/commit.

4.  **[Check lastupdated process field]{.underline}**: This is a custom field that needs to contain the flow name in Dataverse Create/Update actions. This check was built when multiple flows were updating the same tables/records and the users/devs having trouble identifying the flow that made the updates.

    a.  **[Flow auditing attribute]{.underline}**: The name of the attribute which would be traced in all the Dataverse Create/Update actions of flow. This attribute is needed for record auditing purposes in the Environment.

5.  **[Check missing connection references]{.underline}**: Checks if any connection references are missing in the solution in the repo but are referenced in flows.

6.  **[Check publisher]{.underline}**: This rule ensures publisher values are consistent in the org. The configurable values are provided in solutionpublisherprefix, solutionpublisheruniquename, solutionpublisheroptionvalueprefix variables.

7.  **[Check security roles]{.underline}**: This rule logs role permissions in the PR as comments in an easy-to-read tabular format.

8.  **[Check solution dependencies:]{.underline}** List any new dependencies on solutions present in the same repository added in the current PR/commit.

9.  **[Check solution version]{.underline}**: Check if solution version matches the current weekly sprint version.

10. **[Check Web Resources with specific file types]{.underline}**: Check if any new Web Resources with specified file types were added/modified in the PR/commit.

11. **[Enable comments to pull request]{.underline}**: Set this flag true to post comments to PR else comments will be added to only console.

12. **[Exclusion list path]{.underline}**: If a certain list of components, entities, folders, file extensions need to be excluded by the tool, then they should be added here.

13. **[Solution publisher option value prefix]{.underline}**: This is comma separated string (without space) mentioning all allowed option value prefix.

14. **[Solution publisher prefix]{.underline}**: Comma separated string (without space) mentioning all allowed publisher prefix.

15. **[Solution publisher unique name]{.underline}**: Comma separated string (without space) mentioning all allowed publisher unique prefix.

# Pre-requisites

-   Create/Get personal access token (PAT) which will be used to authenticate into Azure DevOps. For more information, see [Personal access tokens (PAT)].

-   URL of organization in Azure DevOps.

-   For code searches, install **Code Search** **Extension** ([Code Search]) from Marketplace extension. For more information, see [Functional code search].

> ![Graphical user interface, application Description automatically generated]

# Setup

PR Checker Tool is an application, for which the executable can be added to the repository and configured as mentioned in the sections below. In our repository it currently sits in a Tools folder.

Below files need to be added in your repository for PR Checker to work (see screenshot below):

-   ExclusionList.xml (DuplicatesComponentTool folder)

-   FocusCenterPRsChecker.exe

-   FocusCenterPRsChecker.exe.config

-   FocusCenterPRsChecker.pdb

-   Newtonsoft.Json.dll

-   Newtonsoft.Json.xml

    ![A screenshot of a computer Description automatically generated with medium confidence]

# Configuration

Post solution import, we must follow steps given below.

1.  ## Create/update pipeline variable group

    1.  ### Sign in to your organization (https://dev.azure.com/{yourorganization}) and select your project.

    2.  ### Select **Pipelines \> Library \> + Variable group.**

    3.  ### Create variable group with name say "FocusCenter- PR Tool" and description for the group

    4.  ### Enter the following variables' name and value for each variable to include in the group, choosing + Add for each one.

    5.  ### When you\'re finished adding variables, select **Save**.

+------------------------------------+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+---------------------------------------------------------+
| Variable Name                      | Description                                                                                                                                                                                            | Values & Example                                        |
+====================================+========================================================================================================================================================================================================+=========================================================+
| checkduplicatecomponents           | Check if any components are duplicate in solution.                                                                                                                                                     | true/false                                              |
+------------------------------------+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+---------------------------------------------------------+
| checkduplicateconnectionreferences | Check if any connection references are duplicate in solution.                                                                                                                                          | true/false                                              |
+------------------------------------+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+---------------------------------------------------------+
| checkentitylevelbusinessrules      | Check if any new/modified entity-level business rules are present.                                                                                                                                     | true/false                                              |
+------------------------------------+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+---------------------------------------------------------+
| checklastupdatedprocessfield       | Check if audit attribute is filled in flows having Dataverse Create/Update actions.                                                                                                                    | true/false                                              |
+------------------------------------+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+---------------------------------------------------------+
| checkmissingconnectionreferences   | Check if any connection references are missing in solution.                                                                                                                                            | true/false                                              |
+------------------------------------+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+---------------------------------------------------------+
| checkpublisher                     | To ensure publisher values are set as values provided in solutionpublisherprefix, solutionpublisheruniquename, solutionpublisheroptionvalueprefix                                                      | true/false                                              |
+------------------------------------+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+---------------------------------------------------------+
| checksecurityroles                 |                                                                                                                                                                                                        | true/false                                              |
+------------------------------------+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+---------------------------------------------------------+
| checksolutiondependencies          | List new dependencies added in the PR/commit on the solutions that exist in the repository.                                                                                                            | true/false                                              |
+------------------------------------+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+---------------------------------------------------------+
| checksolutionversion               | Check if solution version matches to the current sprint version.                                                                                                                                       | true/false                                              |
+------------------------------------+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+---------------------------------------------------------+
| checkwebresourcesfiletypes         | Check if any new/modified Web Resource of specified types are present in PR/commit.                                                                                                                    | For example:\                                           |
|                                    |                                                                                                                                                                                                        | js,html,css                                             |
+------------------------------------+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+---------------------------------------------------------+
| enablecommentstopullrequest        | Set this flag true to post comments to PR else comments will be added to only console.                                                                                                                 | true/false                                              |
|                                    |                                                                                                                                                                                                        |                                                         |
|                                    |                                                                                                                                                                                                        | Default: false                                          |
+------------------------------------+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+---------------------------------------------------------+
| exclusionlistpath                  | String path to the file which contains list of components, entities, folders, file extensions to be excluded.                                                                                          | For example: DuplicatesComponentTool\\ExclusionList.xml |
+------------------------------------+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+---------------------------------------------------------+
| flowauditingattribute              | String name of the attribute which would be traced in all the Dataverse Create/Update actions of flow. This attribute is needed for record auditing purposes in the Environment. Must be in lowercase. | For example: mssh_lastupdatedbyprocess                  |
+------------------------------------+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+---------------------------------------------------------+
| masterbranchname                   | Name of the master branch of the repository.                                                                                                                                                           | For example:\                                           |
|                                    |                                                                                                                                                                                                        | master, develop                                         |
+------------------------------------+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+---------------------------------------------------------+
| solutionpublisheroptionvalueprefix | Comma separated string (without space) mentioning all allowed option value prefix.                                                                                                                     | For example: 28042,28041                                |
+------------------------------------+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+---------------------------------------------------------+
| solutionpublisherprefix            | Comma separated string (without space) mentioning all allowed publisher prefix.                                                                                                                        | For example: msfc,mssh                                  |
+------------------------------------+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+---------------------------------------------------------+
| solutionpublisheruniquename        | Comma separated string (without space) mentioning all allowed publisher unique prefix.                                                                                                                 | For example: MicrosoftSuccessHub                        |
+------------------------------------+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+---------------------------------------------------------+

> ![Graphical user interface, text, email Description automatically generated]

### Create another variable group say "FocusCenter- PR Tool PAT" and enable "**Link secrets from an Azure key vault**". For more information, see [Link secrets from an Azure key vault].

  -------------------------------------------------------------------------
  Variable Name           Description                    Values & Example
  ----------------------- ------------------------------ ------------------
  PAT-shplat              PAT Token                      

  -------------------------------------------------------------------------

*Note: If any of above values are not set then tool will show below error.*

![A screenshot of a computer Description automatically generated]

2.  ## Configure tool in pipeline

    1.  ### Use the variable groups created in *Pre-requisites Section*. Open your pipeline and select **Variables \> Variable groups**, and then choose **Link variable group**. Select the previous created groups.

    2.  ### In pipeline, add *command line* task (Run a command line script using Bash on Linux and macOS and cmd.exe on Windows).

    3.  ### Add *Display name.*

    4.  ### In the *Script* section, add:

        a.  ### Exe name

        b.  ### Relative path to solution repository

        c.  ### PAT variable name from pipeline variables

        d.  ### PullRequestId from system variables.

*Note: Add the above values separated by space.*

Example:

**.\\FocusCenterPRsChecker.exe ..\\..\\..\\Extracted \$(PAT-shplat) \$(System.PullRequest.PullRequestId)**

### Click on *Advanced* and add Path to exe in *Working Directory.*

![Graphical user interface, application Description automatically generated][2]

## View of PR Checker Tool working on pipeline

> Below is the view of PR Checker Tool working on pipeline.

![A screenshot of a computer Description automatically generated][3]

![Graphical user interface, text, application, email Description automatically generated][4]

![Graphical user interface, text, application, email Description automatically generated][5]

  [Introduction 2]: #introduction
  [Components and functionality 2]: #_Toc89418494
  [Pre-requisites 5]: #pre-requisites
  [Setup 5]: #_Toc89418496
  [Configuration 6]: #configuration
  [Graphical user interface, text, application, email Description automatically generated]: media/image1.png {width="3.2840321522309712in" height="2.1697069116360455in"}
  [Table Description automatically generated]: media/image2.png {width="3.3515616797900263in" height="2.531933508311461in"}
  [1]: media/image3.png {width="3.800439632545932in" height="2.6985214348206474in"}
  [Personal access tokens (PAT)]: https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops
  [Code Search]: https://marketplace.visualstudio.com/items?itemName=ms.vss-code-search
  [Functional code search]: https://docs.microsoft.com/en-us/azure/devops/project/search/functional-code-search?view=azure-devops
  [Graphical user interface, application Description automatically generated]: media/image4.png {width="5.015393700787402in" height="2.0978280839895014in"}
  [A screenshot of a computer Description automatically generated with medium confidence]: media/image5.png {width="5.0111154855643045in" height="3.1566907261592303in"}
  [Graphical user interface, text, email Description automatically generated]: media/image6.png {width="5.224872047244094in" height="5.512351268591426in"}
  [Link secrets from an Azure key vault]: https://docs.microsoft.com/en-us/azure/devops/pipelines/library/variable-groups?view=azure-devops&tabs=yaml#link-secrets-from-an-azure-key-vault
  [A screenshot of a computer Description automatically generated]: media/image7.png {width="6.5in" height="2.875in"}
  [2]: media/image8.png {width="5.472222222222222in" height="2.7858584864391953in"}
  [3]: media/image9.png {width="6.5in" height="3.138888888888889in"}
  [4]: media/image3.png {width="6.5in" height="3.0972222222222223in"}
  [5]: media/image10.png {width="6.5in" height="3.0972222222222223in"}
