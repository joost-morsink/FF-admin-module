# Application

The Admin module application supports the execution of the business process.
The relevant process steps are mapped to application components that need to be developed.
The terminology in this chapter is website terminology; the application could also be a native application.

| Proces step                                            | Application component                                    |
| ------------------------------------------------------ | -------------------------------------------------------- |
| Import donation data into the event source             | Page with upload                                         |
|                                                        | Additional information (price info, new charities, etc.) |
|                                                        | Result page                                              |
| Calculate liquidation and transfers                    | Page                                                     |
| Execute liquidation and administer results             | Page + result page                                       |
| Execute Enter/Exit                                     | Database call                                            |
| Execute investment of new funds and administer results | Page + result page                                       |
| Transfer money to charities and administer results     | Page + result page                                       |
| Make an export for auditing purposes                   | Download                                                 |
| Make an export for the website                         | Download                                                 |

Each result page may coincide with the 'next' page in the process.
