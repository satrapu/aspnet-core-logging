Feature: Add todo items
As a user I want to be able to add todo items so I won't forget about important things I need to do each day.

    @add-todo-item
    @positive
    Scenario: Add new todo item using valid input
        Given the current user has the below details
          | UserName         | Password   |
          | acceptance-tests | Qwerty!123 |
        When the current user adds a new todo item using the below details
          | Name           | IsComplete |
          | Add more tests | true       |
        Then the system must reply with a success response
          | HttpStatusCode | LocationHeaderValueMatchExpression |
          | 201            | http*:*//*/api/todo/*              |

    @add-todo-item
    @negative
    Scenario: Unauthenticated user cannot add todo item
        Given the current user is not authenticated
        When the current user adds a new todo item using the below details
          | Name              | IsComplete |
          | Authenticate user | false      |
        Then the system must reply with an error response with status code 401

    @add-todo-item
    @negative
    Scenario: Unauthorized user cannot add todo item
        Given the current user is not authorized
        When the current user adds a new todo item using the below details
          | Name           | IsComplete |
          | Authorize user | false      |
        Then the system must reply with an error response with status code 401
