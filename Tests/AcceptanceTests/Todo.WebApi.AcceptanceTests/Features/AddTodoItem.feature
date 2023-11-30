Feature: Add todo items
As a user I want to be able to add todo items so I won't forget about important things I need to do each day.

    @add-todo-item
    Scenario: Add new todo item
        Given the current user has the below details
          | UserName         | Password   |
          | acceptance-tests | Qwerty!123 |
        When the current user adds a new todo item using the below details
          | Name           | IsComplete |
          | Add more tests | true       |
        Then the system must create the todo item
