# trinug-2019-08-21-workshop

## Fake Your Database

Some of the scariest code to test can sometimes be your database dependency. Rather than let that stop you, let’s walk through some powerful ways to fake your database dependency to quickly and efficiently test code next to it.

In this workshop, we will:

* Create a new ASP .NET Core API project.
* Add an API to get data a pre-existing database
* Add a service project
* Add a model project
* Add a postgres service project
* Connect to a pre-provisioned postgres database
* Add a test project
* Add provider tests which fake the database.

## Creating the Database

This was done before the workshop, but for the sake of transparency, these were the resources used:

* http://www.postgresqltutorial.com/postgresql-sample-database/
* http://www.postgresqltutorial.com/load-postgresql-sample-database/
* https://tableplus.io/blog/2018/04/postgresql-how-to-create-read-only-user.html

The exact scripts will be added to [./data](./data)