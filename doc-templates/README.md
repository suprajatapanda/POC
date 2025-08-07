[//]: # ( IMPORTANT: Edit file in doc-templates folder.  The file in the root folder is generated and replaced by each build; any changes made to the file in the root folder will be lost.)
 
# @project.artifactId@ [![SonarQube Quality Gate](https://sonarqube.shared.tamodel.aegon.io/api/project_badges/measure?project=@project.groupId@%3A@project.artifactId@&metric=alert_status)](https://sonarqube.shared.tamodel.aegon.io/dashboard?id=@project.groupId@%3A@project.artifactId@) [![SonarQube Bugs](https://sonarqube.shared.tamodel.aegon.io/api/project_badges/measure?project=@project.groupId@%3A@project.artifactId@&metric=bugs)](https://sonarqube.shared.tamodel.aegon.io/project/issues?id=@project.groupId@%3A@project.artifactId@&resolved=false&types=BUG) [![SonarQube Code Smell](https://sonarqube.shared.tamodel.aegon.io/api/project_badges/measure?project=@project.groupId@%3A@project.artifactId@&metric=code_smells)](https://sonarqube.shared.tamodel.aegon.io/project/issues?facetMode=effort&id=@project.groupId@%3A@project.artifactId@&resolved=false&types=CODE_SMELL) [![SonarQube Coverage](https://sonarqube.shared.tamodel.aegon.io/api/project_badges/measure?project=@project.groupId@%3A@project.artifactId@&metric=coverage)](https://sonarqube.shared.tamodel.aegon.io/component_measures?id=@project.groupId@%3A@project.artifactId@&metric=coverage) [![SonarQube Security Rating](https://sonarqube.shared.tamodel.aegon.io/api/project_badges/measure?project=@project.groupId@%3A@project.artifactId@&metric=security_rating)](https://sonarqube.shared.tamodel.aegon.io/component_measures?id=@project.groupId@%3A@project.artifactId@&metric=Security)
 
## @project.name@
 
@project.description@

---

This repo contains a Microservice API Pattern application. [Click here](http://bitbucket.us.aegon.com/projects/PLAT/repos/introduction-to-digital-platform/browse) to read more about this and other patterns available from the Digital Platform.

#### How to run this application locally.
    mvn spring-boot:run -P dev-vault -Dspring.profiles.active=local,dev

