using System;
using System.Collections;
using System.Collections.Generic;
using ApprovalTests;
using ApprovalTests.Reporters;
using Coulda.Test;
using HtmlTags;
using Xunit;

namespace SharpViews.Tests
{
    public class LoginView
    {
        private HtmlTag _content;

        public LoginView()
        {
           


        }

        public bool EmailNotConfirmed { get; set; }

        public bool CanRegister { get; set; }

        public string ResendEmailUrl { get; set; }

        public string RecoverPasswordUrl { get; set; }

        public string LoginUrl { get; set; }

        private HtmlTag row(params HtmlTag[] children)
        {
            var tag = new HtmlTag("div");
            tag.AddClass("row");
            tag.Append(children);
            return tag;
        }

        private HtmlTag col(int? md = null, int? xs = null, int? sm = null, int? lg = null, int? mdOff = null,
            int? smOff = null, int? xsOff = null, int? lgOff = null,IEnumerable<HtmlTag> children = null)
        {
            var tag = new HtmlTag("div");
            if (md.HasValue)
            {
                tag.AddClass("col-md-" + md.Value);
            }
            if (xs.HasValue)
            {
                tag.AddClass("col-xs-" + xs.Value);
            }
            if (sm.HasValue)
            {
                tag.AddClass("col-sm-" + sm.Value);
            }
            if (lg.HasValue)
            {
                tag.AddClass("col-lg-" + lg.Value);
            }
            if (mdOff.HasValue)
            {
                tag.AddClass("col-md-offset-" + mdOff.Value);
            }
            if (smOff.HasValue)
            {
                tag.AddClass("col-sm-offset-" + smOff.Value);
            }
            if (xsOff.HasValue)
            {
                tag.AddClass("col-xs-offset-" + xsOff.Value);
            }
            if (lgOff.HasValue)
            {
                tag.AddClass("col-lg-offset-" + lgOff.Value);
            }

            tag.Append(children);
            return tag;
        }

        private HtmlTag strong(string txt)
        {
            return new HtmlTag("strong").Text(txt);
        }

        private HtmlTag p(string text)
        {
            return new HtmlTag("p").Text(text);
        }

        private HtmlTag a(string href, string text, string[] classes)
        {
            return new HtmlTag("a").Attr("href",href).Text(text).AddClasses(classes);
        }

        public HtmlTag Render()
        {
            _content = new HtmlTag("section");
            _content.Id("login_main");

            if (EmailNotConfirmed)
            {
                
                //_content.Append(
                //  row(
                //    col(mdOff: 2, md: 6, children:new[]{
                //        strong("Important!"),
                //        p("You must confirm your email address before you can sign in to this site."),
                //        a(href: ResendEmailUrl, text: "Resend Email", classes:new [] {"btn","btn-primary"})
                //       })
                //    )
                //);

                var resendView = new ResendView();
                resendView.Url = ResendEmailUrl;
                _content.Append(resendView);


                //_content.Add<DivTag>().AddClass("row").Append(new[]
                //{
                //    new HtmlTag("div").AddClasses("col-md-offset-2","col-md-6").Append(new[]
                //    {
                //        new HtmlTag("strong").Text("Important!"),
                //        new HtmlTag("p").Text("You must confirm your mail address before you can sign in to this site."), 
                //        new HtmlTag("a").Attr("href", ResendEmailUrl).AddClasses("btn", "btn-primary").Text("Resend Email"),
                //    }), 
                //});
                //_content.Add("div", row =>
                //{
                //    row.AddClass("row");
                //    row.Add("div", col =>
                //    {
                //        col.AddClasses("col-md-offset-2", "col-md-6");
                //        col.Add("strong").Text("Important!");
                //        col.Add("p").Text("You must confirm your email address before you can sign in to this site.");
                //        col.Add("a")
                //            .Attr("href", ResendEmailUrl)
                //            .AddClasses("btn", "btn-primary")
                //            .Text("Resend Email");
                //    });
                //});
            }

            _content.Append("div", row =>
            {
                row.AddClass("row");
                row.Add("div", col =>
                {
                    col.AddClass("col-md-4");
                    //TODO:HtmlHelperValidationSummary

                    col.Append("form", loginForm =>
                    {
                        loginForm.Attr("action", LoginUrl);
                        loginForm.Attr("method", "POST");
                        loginForm.Attr("autocomplete", "off");
                        loginForm.AddClass("form-stacked");

                        loginForm.Add("div", fg =>
                        {
                            fg.AddClass("form-group");
                            fg.Add("label").Attr("for", "username").Text("Username").AddClass("control-label");
                            fg.Add("input")
                                .Attr("type", "text")
                                .Attr("name", "username")
                                .AddClass("form-control");

                        });

                        loginForm.Add("div", fg =>
                        {
                            fg.AddClass("form-group");
                            fg.Add("label").Attr("for", "password").Text("password").AddClass("control-label");
                            fg.Add("input")
                                .Attr("type", "text")
                                .Attr("name", "password")
                                .AddClass("form-control");

                        });

                        loginForm.Add("div", fg =>
                        {
                            fg.AddClass("form-group");
                            fg.Add("div")
                                .Add("button").Attr("type", "submit").AddClasses("btn", "btn-primary").Text("Login")
                                .Parent
                                .Add("a").Attr("href", RecoverPasswordUrl).Text("Forgot Password?");

                        });
                    });

                });

                row.Add("div", col =>
                {
                    col.AddClasses("col-md-offset-1", "col-md-3");
                    if (CanRegister)
                    {
                        col.Add("div").Style("margin-top", "20px")
                            .Add("a").Attr("href", RegisterUrl).AddClasses("btn", "btn-success", "btn-block", "btn-lg").Text("Sign Up")
                            .Parent
                            .Add("p").AddClass("help-block").Text("You will need your most recent invoice");
                    }

                    col.Add("div").AppendHtml(SslLogoContent);
                });

            });
            

            return _content;

        }

        public string RegisterUrl { get; set; }

        public string SslLogoContent { get; set; }
    }

    public class ResendView : ContentTag
    {
        protected override void Content()
        {
            row(() =>
            {
                col(mdOff: 2,md: 6,children: () =>
                {
                    div("alert alert-warning", () =>
                    {
                        strong("Important!");
                        p(@"You must confirm your email address before you can sign in to
                            this site.");
                        a(href:Url,text:"Resend Email",classes:new[] {"btn","btn-primary"});
                    });
                });
            });
        }

        public string Url { get; set; }
    }

    public class ContentTag : ITagSource
    {
        private readonly HtmlTag _rootTag;
        private readonly Stack<HtmlTag> _currentScope;

        public ContentTag()
        {
            _rootTag = HtmlTag.Placeholder();
            _currentScope = new Stack<HtmlTag>();
            _currentScope.Push(_rootTag);
        }

        public void row(Action children)
        {
            var tag = new HtmlTag("div");
            tag.AddClass("row");

            _currentScope.Peek().Append(tag);
            _currentScope.Push(tag);
            children();
            _currentScope.Pop();

        }

        public void col(int? md = null, int? xs = null, int? sm = null, int? lg = null, int? mdOff = null,
            int? smOff = null, int? xsOff = null, int? lgOff = null, Action children =null)
        {
            var tag = new HtmlTag("div");
            if (md.HasValue)
            {
                tag.AddClass("col-md-" + md.Value);
            }
            if (xs.HasValue)
            {
                tag.AddClass("col-xs-" + xs.Value);
            }
            if (sm.HasValue)
            {
                tag.AddClass("col-sm-" + sm.Value);
            }
            if (lg.HasValue)
            {
                tag.AddClass("col-lg-" + lg.Value);
            }
            if (mdOff.HasValue)
            {
                tag.AddClass("col-md-offset-" + mdOff.Value);
            }
            if (smOff.HasValue)
            {
                tag.AddClass("col-sm-offset-" + smOff.Value);
            }
            if (xsOff.HasValue)
            {
                tag.AddClass("col-xs-offset-" + xsOff.Value);
            }
            if (lgOff.HasValue)
            {
                tag.AddClass("col-lg-offset-" + lgOff.Value);
            }

            _currentScope.Peek().Append(tag);
            _currentScope.Push(tag);
            children();
            _currentScope.Pop();
        }

        public void strong(string txt)
        {
            var tag =  new HtmlTag("strong").Text(txt);
            _currentScope.Peek().Append(tag);
        }

        public void p(string text)
        {
            var tag =  new HtmlTag("p").Text(text);
            _currentScope.Peek().Append(tag);
        }

        public void a(string href, string text, string[] classes)
        {
            var tag  = new HtmlTag("a").Attr("href", href).Text(text).AddClasses(classes);
            _currentScope.Peek().Append(tag);

        }

        public void div(string classes, Action children)
        {
            var tag = new HtmlTag("div");
            var classesArr = classes.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var c in classesArr)
            {
                tag.AddClass(c);
            }
            _currentScope.Peek().Append(tag);
            _currentScope.Push(tag);
            children();
            _currentScope.Pop();
        }

        public IEnumerable<HtmlTag> AllTags()
        {
            Content();
            yield return _rootTag;
        }

        protected virtual void Content()
        {
            
        }
    }
    public class PageViewTests : CouldaBase
    {
        [Fact]
        [UseReporter(typeof(ApprovalTests.Reporters.VisualStudioReporter))]
        public void Verify_login_view_html()
        {
            var view = new LoginView()
            {
                CanRegister = true,
                LoginUrl = "/login",
                RegisterUrl = "/register",
                ResendEmailUrl = "/resend",
                EmailNotConfirmed = true,
                RecoverPasswordUrl = "/recover",
                SslLogoContent = "<h1>YOUR LOGO</h1>"
            };
            Approvals.Verify(view.Render().ToPrettyString());
        }
    }
}