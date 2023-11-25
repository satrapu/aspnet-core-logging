Feature: Add todo item
As a user I want to be able to add todo items so I won't forget about important things I need to do each day.

    @add-todo-item
    Scenario: Add new todo item
        Given the current user is authorized to add a new todo item
        When the current user adds a new todo item using the below details
          | Name           | Description    |
          | Do the laundry | Do the laundry |
        Then the system must store that todo item
