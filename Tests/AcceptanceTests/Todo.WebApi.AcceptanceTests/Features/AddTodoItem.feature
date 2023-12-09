Feature: Add todo items
As a user I want to be able to add new todo items so I won't forget about the important things I need to take care of each day.

    @add-new-todo-item
    @expected-positive-result
    Scenario: Add new todo item
        Given the current user is authorized to add todo items
        When the current user tries adding a new todo item
          | Name                      | IsComplete |
          | Add more acceptance tests | true       |
        Then the system must add the new todo item
        And reply with a success response
          | HttpStatusCode | LocationHeaderValueMatchExpression |
          | 201            | http*:*//*/api/todo/*              |

    @add-new-todo-item
    @expected-negative-result
    Scenario: Unauthenticated user cannot add todo items
        Given the current user is not authenticated
        When the current user tries adding a new todo item
          | Name                    | IsComplete |
          | Authenticate user first | false      |
        Then the system must not add the new todo item
        And reply with a failed response
          | HttpStatusCode |
          | 401            |

    @add-new-todo-item
    @expected-negative-result
    Scenario: Unauthorized user cannot add todo items
        Given the current user is not authorized to add todo items
        When the current user tries adding a new todo item
          | Name                                                        | IsComplete |
          | Grant the appropriate permissions to the current user first | false      |
        Then the system must not add the new todo item
        And reply with a failed response
          | HttpStatusCode |
          | 403            |
