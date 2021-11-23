# Application

## Overview

The Admin module application supports the execution of the business process.
The relevant process steps are mapped to application components that need to be developed.
The terminology in this chapter is website terminology; the application is actually a SPA (single page application).

| Process step                                           | Application component                                    |
| ------------------------------------------------------ | -------------------------------------------------------- |
| Calculate liquidation and transfers                    | Page                                                     |
| Execute liquidation and administer results             | Page + result page                                       |
| Execute Exit                                           | Probably no UI needed (maybe just a button)              |
| Transfer money to charities and administer results     | Page + result page                                       |
| Import donation data into the event source             | Page with upload                                         |
|                                                        | Additional information (price info, new charities, etc.) |
|                                                        | Result page                                              |
| Execute Enter                                          | Probably no UI needed (maybe just a button               |
| Execute investment of new funds and administer results | Page + result page                                       |
| Make an export for auditing purposes                   | Download                                                 |
| Make an export for the website                         | Download                                                 |

Each result page may coincide with the 'next' page in the process.

The application layer will not contain any complex logic, this will be left to the database layer.

## Initialization

The application should be able to initialize a clean slate database by processing all the events since the start of the administration.
This basically means piping all the events to the database and letting it handle them.