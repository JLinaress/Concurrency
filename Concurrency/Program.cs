// Implementing a simple lower level Async / Await from scratch 
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

// First example 
// Uncomment to see how we use ThreadPool.QueueUserWorkItem
// Note:
//this is not thread safe, the output will be jumbled.
// Will fix in next example.
// Demonstrates the use of ThreadPool to execute multiple tasks concurrently.
// Each task prints its thread number and sleeps for 1 second.
// The main thread waits for user input before exiting to ensure all tasks complete.
// for (int i = 0; i < 20; i++)
// {
//     var currentValue = i; // Capture the current value of i, without this it will always be 20
//     
//     ThreadPool.QueueUserWorkItem(delegate
//     {
//         Console.WriteLine($"Thread {currentValue}"); //original i is always 20. update to currentValue to fix. But this
//         // is not thread safe, so the output will be jumbled.
//         Thread.Sleep(1000);
//     });
// }
// Console.ReadLine();


// Second example
// Uncomment to see how we implement our own thread pool.
// Console.WriteLine("Starting MyThreadPool...");
// Thread.Sleep(2000);

// Implementing a custom thread pool to manage and execute tasks concurrently.
// This custom thread pool uses BlockingCollection to queue work items and a fixed number of worker threads
// to process them. Each task prints its thread number and sleeps for 1 second.
// The main thread waits for user input before exiting to ensure all tasks complete.
// for (int i = 0; i < 20; i++)
// {
//     var currentValue = i; // Capture the current value of i, without this it will always be 20
//     
//     // QueueUserWorkItem is low level API, it can fork, but it doesn't join. So we can't wait for all the tasks to complete.
//     MyThreadPool.QueueUserWorkItem(() => // delegate. Using lambda expression instead of delegate. But both work.
//     {
//         Console.WriteLine($"MyThreadPool Thread {currentValue}"); //original i is always 20. update to currentValue to fix. But this
//         // is not thread safe, so the output will be jumbled.
//         Thread.Sleep(1000);
//     });
// }
// Console.ReadLine();



// Third example
// Uncomment to see how we implement our own Task class.
// Console.WriteLine("Starting MyTask...");
// Thread.Sleep(2000);

// Implementing a simple Task class to manage asynchronous operations.
// This Task class allows queuing of work items, waiting for their completion, and handling exceptions.
// Each task prints its thread number and sleeps for 1 second.
// AsyncLocal<int> asyncLocal = new();
// List<MyTask> tasks = new();
// for (int i = 0; i < 20; i++)
// {
//     asyncLocal.Value = i; // Capture the current value of i, without this it will always be 20
//     
//     tasks.Add(MyTask.Run(delegate
//     {
//         Console.WriteLine($"My Task Thread {asyncLocal.Value}"); //original i is always 20. update to currentValue to fix. But this
//         // is not thread safe, so the output will be jumbled.
//         Thread.Sleep(1000);
//     }));
// }
// // now instead of waiting for user input, I want to wait for all the tasks to complete.
// foreach (var task in tasks) task.Wait();
// // Instead of waiting for each task individually, I could also implement a WhenAll method to wait for all tasks to complete.
// // This would be similar to Task.WhenAll in the TPL.
// MyTask.WhenAll(tasks).Wait();


// Fourth example
// Uncomment to see how we implement Delay method in our own Task class.
// Console.WriteLine("Starting MyTask with Delay...");
// Thread.Sleep(2000);

// // Implementing a simple Task class with a Delay method to manage asynchronous operations.
// Console.Write("Hello, ");
// MyTask.Delay(3000).ContinueWith(delegate
// {
//     Console.Write("World!");
// }).Wait(); // Wait for the continuation to complete before exiting the main thread.
// Console.ReadLine();


// Fifth example
// Uncomment to see how we implement async / await using our own Task class.
// Sort of a Link List example using the overloaded ContinueWith method.
// Console.WriteLine("Starting MyTask with Async/Await...");
// Thread.Sleep(2000);

// Implementing a simple Task class with chained linked functionality to manage asynchronous operations.
// This example demonstrates chaining of tasks using ContinueWith to mimic chained linked behavior.
// Console.Write("Hello...");
// MyTask.Delay(3000).ContinueWith(delegate
// {
//     Console.Write(" World");
//     return MyTask.Delay(2000); // Return a new MyTask to chain the next continuation.
// }).ContinueWith(delegate
// {
//     Console.Write(" And Juan!!!");
// }).Wait(); // Wait for the final continuation to complete before exiting the main thread.


// Sixth example
// Uncomment to see how we implement async / await using our own Task class with IEnumerable.
// This is similar to using yield return in C#.
// Console.WriteLine("Starting MyTask with IEnumerable...");
// Thread.Sleep(2000);

// Implementing a simple Task class with IEnumerable to manage asynchronous operations.
// This example demonstrates iterating over a collection of tasks using an enumerator to mimic async/await behavior.
// Console.WriteLine("Start the count down...");
// MyTask.Iterate(PrintAsync()).Wait(); // Wait for the iteration to complete before exiting the
// static IEnumerable<MyTask> PrintAsync()
// {
//     for (int i = 10; i >= 0 ; i--)
//     {
//         yield return MyTask.Delay(1000); // Wait for 1 second
//         if (i == 0) 
//         {
//             Console.WriteLine("Blast off!");
//             continue;
//         }
//         Console.WriteLine(i); // Print digits 0-9 in a loop
//     }
// }


// Finally, lets use await 
// Uncomment to see how we implement async / await using our own Task class with Awaiter.
//  This is similar to using yield return in C#.
//  This is the most natural way to write asynchronous code in C#.
Console.WriteLine("Starting MyTask with Await...");
Thread.Sleep(2000);
//
// // Implementing a simple Task class with Awaiter to manage asynchronous operations.
// // This example demonstrates the use of the await keyword to asynchronously wait for a task to complete.
// // The Awaiter class provides the necessary methods to support the await keyword.
PrintAsync().Wait(); // Wait for the iteration to complete before exiting the
static async Task PrintAsync()
 {
    for (int i = 10; i >= 0 ; i--)
    {
        await MyTask.Delay(1000); // Wait for 1 second
        if (i == 0) 
        {
            Console.WriteLine("Blast off!");
            continue;
        }
        Console.WriteLine(i); 
    }
 }

// Creating my own thread pool
// This is a simple implementation of a thread pool using BlockingCollection and Task.
// It allows queuing of work items and processes them using a fixed number of worker threads.
static class MyThreadPool
{
    // Concurrent queue to hold the work items and when I want to take one out, it will block until there is something to take out.
    // I want this thread-pool to wait until there is something to take out.
    private static readonly BlockingCollection<(Action, ExecutionContext?)> s_workItems = new();
    
    // An Action is a delegate that's already has been defined. It is a method that takes no parameters and returns void.
    // If I wanted to pass parameters, I would use Func<T> instead of Action.
    // public static void QueueUserWorkItem(Action action) => s_workItems.Add((action, ExecutionContext.Capture()));
    
    // updated to capture the execution context as well.
    public static void QueueUserWorkItem(Action action) => s_workItems.Add((action, ExecutionContext.Capture()));
    
    static MyThreadPool()
    {
        // Create a number of worker threads equal to the number of processors on the machine.
        // Each thread will run in the background and will keep running until the application exits.
        // Environment.ProcessorCount gives the number of processors on the machine.
        for (int i = 0; i < Environment.ProcessorCount; i++)
        {
            // kick off a new thread to process work items.
            new Thread(() =>
            {
                while (true)
                {
                    // taking the next work item from the queue and executing it. 
                    // Updated to use ExecutionContext.Run to flow the execution context.
                    // This ensures that any context (like security, culture, etc.) is preserved when the
                    (Action workItem, ExecutionContext context) = s_workItems.Take();

                    if (context is null)
                    { 
                        // Execute the work item
                        workItem();
                    }
                    else
                    {
                        // Execute the work item within the captured execution context
                        // This ensures that the context (like security, culture, etc.) is preserved.
                        ExecutionContext.Run(context, state => ((Action)state!).Invoke(), workItem);
                    }
                   
                }
                // IsBackground = true means that the thread will not prevent the process from terminating.
                // If all foreground threads have terminated, the process will end and any remaining background threads will be stopped.
            }){ IsBackground = true }.Start();
        }
    }
}

// I want to be able to queue work items and then have them join back to the main thread.That's why we have Task.
// This is a simple representation of a task which we can layer on top of the MyThreadPool class. 
// Task is a data structure that sits in memory, that you can do a few operations on it, like wait for it to complete.
class MyTask
{
    private bool _completed;
    private Exception? _exception;
    private Action? _continuation;
    private ExecutionContext? _context;

    public struct Awaiter(MyTask _task) : INotifyCompletion
    {
        public Awaiter GetAwaiter() => this;
        
        public bool IsCompleted => _task.IsCompleted;

        public void OnCompleted(Action continuation) => _task.ContinueWith(continuation);

        public void GetResult() => _task.Wait();
    }
    
    public Awaiter GetAwaiter() => new(this);
        
    // check whether the task is completed .
    private bool IsCompleted
    {
        get
        {
            // Everyone get behind this lock statement, so that only one thread can access the _completed variable at a time.
            lock(this) // never lock(this) in real world code. This is just for simplicity.
            {
                    return _completed;
            }
        }
    } 
        
    // set result of the task. this is usually done in TaskCompletionSource. But for simplicity, we are creating it here.
    public void SetResult() => Complete(null); 
        
    // Check where it failed. this is usually done in TaskCompletionSource. But for simplicity, we are creating it here.
    public void SetException(Exception exception) => Complete(exception);

    // Helper method to mark the task as completed. this is usually done in TaskCompletionSource. But for simplicity, this method will take an exception.
    private void Complete(Exception? exception)
    {
        lock (this) // never lock(this) in real world code. This is just for simplicity.
        {
            if (_completed) throw new InvalidOperationException("Already completed, no need to keep going.");
                
            _completed = true; // mark the task as completed
            _exception = exception; // store the exception if any

            // If there is a continuation, we need to run it.
            // We will run it on a thread pool thread to avoid blocking the current thread.
            // We will also capture the execution context to flow it to the continuation.
            if (_continuation is not null)
            {
                MyThreadPool.QueueUserWorkItem(delegate
                {
                    if (_context is null)
                    {
                        _continuation();
                    }
                    else
                    {
                        // Execute the continuation within the captured execution context
                        // This ensures that the context (like security, culture, etc.) is preserved.
                        ExecutionContext.Run(_context, state => ((Action)state!).Invoke(), _continuation);
                    }
                });
            }
        }
    }
        
    // wait for the task to complete. this is usually done in TaskCompletionSource. But for simplicity, we are creating it here.
    public void Wait()
    {
        // I need to wait / block until the task is completed.
        // I can use ManualResetEventSlim for this purpose.
        ManualResetEventSlim? mres = null;

        lock (this)
        {
            if (!_completed)
            {
                mres = new ManualResetEventSlim(); // create a new ManualResetEventSlim if the task is not completed
                ContinueWith(mres.Set); // Need to signal, anyone waiting on this task, wake them up.
            }
        }

        mres?.Wait();
            
        // If there was an exception, throw it.
        if (_exception is not null)
        {
                // ExceptionDispatchInfo.Throw(_exception); low level type. Takes exception but does not overwrite the stack trace.
                throw new AggregateException(_exception); // wrap the exception in an AggregateException to mimic Task. Great for postmortem debugging.
        }
    }
        
    // If I don't want to asynchronously block and want a callback/notification when the task is completed.
    // Will give it a delegate to call when the task is completed.
    public MyTask ContinueWith(Action action)
    {
        // this is the logic to await the task. in the fourth example, we will use this to implement Delay.
        MyTask task = new();

        // this is invoking the action when the task is completed.
        Action callback = () =>
        {
            // If the action throws an exception, we need to capture it and set it on the new task.
            try
            {
                action();
            }
            catch (Exception e)
            {
                task.SetException(e);
                return;
            }
            
            task.SetResult();
        };
        lock (this) // never lock(this) in real world code. This is just for simplicity.
        {
            // if it completed, queue the work item.
            if (_completed)
            {
                // If the task is already completed, run the continuation immediately.
                MyThreadPool.QueueUserWorkItem(callback);
            }
            else
            {
                _continuation = callback; // store the continuation to run it later when the task is completed.
                _context = ExecutionContext.Capture(); // capture the execution context to flow it to the
            }
        }

        return task;
    } 
    
    // Overloaded ContinueWith method to support Func<MyTask> for chaining tasks.
    // This allows for chaining tasks in a way that mimics async/await behavior.
    public MyTask ContinueWith(Func<MyTask> action)
    {
        // this is the logic to await the task. in the fifth example, we will use this to implement Delay.
        MyTask task = new();

        // this is invoking the action when the task is completed.
        Action callback = () =>
        {
            // If the action throws an exception, we need to capture it and set it on the new task.
            try
            {
                // Call the function to get the next task.
                MyTask next = action();
                next.ContinueWith(delegate
                {
                    // When the next task completes, we need to propagate its result or exception to the outer task.
                    if (next._exception is not null)
                    {
                        // If the next task failed, propagate the exception.
                        task.SetException(next._exception);
                    }
                    else
                    {
                        // If the next task succeeded, mark the outer task as completed.
                        task.SetResult();
                    }
                });
            }
            catch (Exception e)
            {
                task.SetException(e);
                return;
            }
            
            task.SetResult();
        };
        lock (this) // never lock(this) in real world code. This is just for simplicity.
        {
            // if it completed, queue the work item.
            if (_completed)
            {
                // If the task is already completed, run the continuation immediately.
                MyThreadPool.QueueUserWorkItem(callback);
            }
            else
            {
                _continuation = callback; // store the continuation to run it later when the task is completed.
                _context = ExecutionContext.Capture(); // capture the execution context to flow it to the
            }
        }

        return task;
    } 
    
    // Queues a work item to be executed and returns a MyTask representing that work item.
    // This is similar to Task.Run in the TPL.
    public static MyTask Run(Action action)
    {
        MyTask task = new();
            
        MyThreadPool.QueueUserWorkItem(delegate
        {
            try
            {
                action(); // Execute the action
                task.SetResult(); // Mark the task as completed successfully
            }
            catch (Exception ex)
            {
                task.SetException(ex); // Mark the task as completed with an exception
            }
        });
            
        return task;
    }

    // Waits for all the provided MyTask instances to complete.
    // This is similar to Task.WhenAll in the TPL.
    public static MyTask WhenAll(List<MyTask> tasks)
    {
        // If there are no tasks, return a completed task.
        MyTask task = new();

        if (tasks.Count == 0)
        {
            task.SetResult(); // If there are no tasks, complete immediately
        }
        else
        {
            // Loop through to count how many tasks are remaining.
            // When the count reaches zero, we know all tasks are completed.
            int remaining = tasks.Count;
            Action continuation = () =>
            {
                // Decrement the count of remaining tasks atomically.
                if (Interlocked.Decrement(ref remaining) == 0) // lightweight atomic operation to decrement the count.
                {
                    // All tasks completed
                    task.SetResult();
                }
            };
            
            // Attach the continuation to each task.
            foreach (var t in tasks)
            {
                t.ContinueWith(continuation);
            }
        }
        
        return task;
    }

    // Creates a MyTask that completes after a specified time delay.
    // This is similar to Task.Delay in the TPL.
    // This is just another helper
    public static MyTask Delay(int timeout)
    {
        MyTask task = new();
        new Timer(_ => task.SetResult()).Change(timeout, -1); // -1 means do not repeat.
        
        return task;
    }

    public static MyTask Iterate(IEnumerable<MyTask> tasks)
    {
        MyTask task = new();
        
        var enumerator = tasks.GetEnumerator();
        
        // Local function to move to the next task in the enumerator.
        void MoveNext()
        {
            try
            {
                if (enumerator.MoveNext()) // if there is a next task
                {
                    var nextTask = enumerator.Current; // get the current enumerator task
                    nextTask.ContinueWith(MoveNext); // when the current task completes, move to the next task

                    return;
                }
            }
            catch (Exception ex)
            {
                task.SetException(ex); // if there is an exception, set it on the outer task
                return;
            }

            task.SetResult(); // if there are no more tasks, complete the outer task
        }
        
        MoveNext();
        
        return task;
    }
}