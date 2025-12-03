using DesafioDev.Application.Common.Exceptions;
using DesafioDev.Application.TodoLists.Commands.CreateTodoList;
using DesafioDev.Application.TodoLists.Commands.PurgeTodoLists;
using DesafioDev.Domain.Entities;

namespace DesafioDev.Application.FunctionalTests.TodoLists.Commands;

using static Testing;

public class PurgeTodoListsTests : BaseTestFixture
{
    [Test]
    public async Task ShouldDeleteAllLists()
    {
        await SendAsync(new CreateTodoListCommand
        {
            Title = "New List #1"
        });

        await SendAsync(new CreateTodoListCommand
        {
            Title = "New List #2"
        });

        await SendAsync(new CreateTodoListCommand
        {
            Title = "New List #3"
        });

        await SendAsync(new PurgeTodoListsCommand());

        var count = await CountAsync<TodoList>();

        count.ShouldBe(0);
    }
}
