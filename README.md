# DotNetEnglishP3
Student repo for project 3 of the Back-end developer: .NET path

This project has a supporting database which will be created the first time the application is run. In order for the database to be correctly
created, you must meet the prerequisites below and change the connection strings to point to the MSSQL Server running on you your local PC.

**Prerequisites**: MSSQL Server Standard 2017 or Express 2017 has been installed and Microsoft SQL Server Managament Studio has been installed).

*Note: earlier versions of MSSQL Server should work fine, but have not been tested.

*In the P3AddNewFunctionalityDotNetCore project, open the appsettings.json file.*

You will see the ConnectionStrings section which defines the connection strings for the 2 databases used in this application.


      "ConnectionStrings":
      {
        "P3Referential": "Server=.;Database=P3Referential-2f561d3b-493f-46fd-83c9-6e2643e7bd0a;Trusted_Connection=True;MultipleActiveResultSets=true",
        "P3Identity": "Server=.;Database=Identity;Trusted_Connection=True;MultipleActiveResultSets=true"
      }
  

**P3Referential** - connection string to the application database.

**P3Identity** - connection string to the user database. This is a separate database as an organization will typically have many different applications. Rather
than have each application define its own users (and many different logins and passwords for a single user), the organization can have just one user database and use permissions and roles to define what applications the 
user has access to. This way a user can access any of the organizations applications with the same login and password and will only be given access to databases and actions defined in the user database.

There are different versions of MSSQL (please use MSSQL for this project and not another database) and when setting up the database server, there are different options that will change the configuration, so the connection strings defined may not work.

The connection strings defined in the project are setup for MSSQL Server Standard 2017. The installation has not created an instance name, so the server is just referred to as "." which means the default instance of MSSQL server
running on the current machine. The built in windows user which should be setup in MSSQL server by default during installation.

If you have installed MSSQL server express, the value to use for Server is most likely .\SQLEXPRESS, so for example, your P3Referential connection string would be: -

    "P3Referential": "Server=.\SQLEXPRESS;Database=P3Referential-2f561d3b-493f-46fd-83c9-6e2643e7bd0a;Trusted_Connection=True;MultipleActiveResultSets=true"
	
If you have trouble connecting, first try to connection using Microsoft SQL Server Managament Studio (make sure authentication type  is "Windows Authentication"), or refer to https://sqlserver-help.com/2011/06/19/help-whats-my-sql-server-name/.
If you still have trouble, please ask your mentor for assistance.
