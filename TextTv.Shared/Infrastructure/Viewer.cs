using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TextTv.Shared.Infrastructure.Contracts;
using TextTv.Shared.Model;

namespace TextTv.Shared.Infrastructure
{
    public class Viewer
    {
        private readonly TextTvMode mode;
        private readonly IHtmlParserFactory _htmlParserFactory;
        private readonly PageNumberHandler pageNumberHandler;
        private readonly ApiCaller apiCaller;
        private ResponseResult responseResult;
        private Exception exception = null;
        private readonly List<Task> tasks;
        private string markup;
        private readonly CancellationTokenSource tokenSource;
        private readonly CancellationToken token;

        private Viewer(TextTvMode mode, IHtmlParserFactory htmlParserFactory, PageNumberHandler pageNumberHandler)
        {
            apiCaller = new ApiCaller();
            tasks = new List<Task>();
            this.mode = mode;
            _htmlParserFactory = htmlParserFactory;
            this.pageNumberHandler = pageNumberHandler;
            this.tokenSource = new CancellationTokenSource();
            this.token = this.tokenSource.Token;
        }

        public static Viewer Initialize(TextTvMode mode, IHtmlParserFactory htmlParserFactory, PageNumberHandler pageNumberHandler)
        {
            Viewer viewer = new Viewer(mode, htmlParserFactory, pageNumberHandler);
            return viewer;
        }

        public Viewer GetRawHtml()
        {
            Action action = () =>
            {
                try
                {
                    if (this.mode == TextTvMode.Tv)
                    {
                        responseResult = apiCaller.GetSvtTextForTv(pageNumberHandler.CurrentPage).Result;
                    }
                    else
                    {
                        responseResult = apiCaller.GetSvtTextForWeb(pageNumberHandler.CurrentPage).Result;
                    }
                }
                catch (Exception exc)
                {
                    this.exception = exc;
                }
            };

            this.tasks.Add(new Task(action, this.token));

            return this;
        }

        public Viewer FlowActions(Action ifException = null, Action ifNoContent = null, Action ifValidContent = null)
        {
            if (ifException != null)
            {
                this.IfException(ifException);
            }

            if (ifNoContent != null)
            {
                this.IfNoContent(ifNoContent);
            }

            if (ifValidContent != null)
            {
                this.IfValidContent(ifValidContent);
            }

            return this;
        }

        private Viewer IfException(Action action)
        {
            Task newTask = new Task(() =>
            {
                if (this.exception != null)
                {
                    action();
                    this.tokenSource.Cancel();
                }
            }, this.token, TaskCreationOptions.AttachedToParent);

            var lastTask = this.tasks.Last();
            lastTask.ContinueWith(task => newTask.Start(), TaskContinuationOptions.AttachedToParent);
            this.tasks.Add(newTask);

            return this;
        }

        private Viewer IfNoContent(Action action)
        {
            Task newTask = new Task(() =>
            {
                if (responseResult.Markup.ToLowerInvariant().Contains("sidan ej i sändning"))
                {
                    action();
                    this.tokenSource.Cancel();
                }
            }, this.token, TaskCreationOptions.AttachedToParent);

            var lastTask = this.tasks.Last();
            lastTask.ContinueWith(task =>
            {
                if (newTask.IsCanceled == false)
                {
                    newTask.Start();
                }
            }, TaskContinuationOptions.AttachedToParent);
            this.tasks.Add(newTask);

            return this;
        }

        private Viewer IfValidContent(Action action)
        {
            Task newTask = new Task(action, this.token, TaskCreationOptions.AttachedToParent);
            

            var lastTask = this.tasks.Last();
            lastTask.ContinueWith(task =>
            {
                if (newTask.IsCanceled == false)
                {
                    newTask.Start();
                }
            }, TaskContinuationOptions.AttachedToParent);
            this.tasks.Add(newTask);

            return this;
        }

        public void ParseForView(Action<string> onDone)
        {
            Task newTask = new Task(() =>
            {
                HtmlParser parser = _htmlParserFactory.CreateParser(responseResult.Markup);
                if (this.mode == TextTvMode.Tv)
                {
                    markup = parser.ParseForTvAsync().Result;
                }
                else
                {
                    markup = parser.ParseForWebAsync().Result;
                }
            }, this.token, TaskCreationOptions.AttachedToParent);

            var lastTask = this.tasks.Last();
            lastTask.ContinueWith(task =>
            {
                if (newTask.IsCanceled == false)
                {
                    newTask.Start();
                }
            }, TaskContinuationOptions.AttachedToParent);
            this.tasks.Add(newTask);

            this.ProcessAsync(onDone);
        }

        public void ProcessAsync(Action<string> onDone)
        {
            var lastTask = this.tasks.Last();
            lastTask.ContinueWith(task =>
            {
                if (task.IsCanceled == false)
                {
                    onDone(this.markup);
                }
            }, TaskContinuationOptions.AttachedToParent);
            
            Task firstTask = this.tasks.First();
            firstTask.Start();
        }
    }
}
