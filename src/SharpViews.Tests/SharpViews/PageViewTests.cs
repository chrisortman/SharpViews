using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI.HtmlControls;
using ApprovalTests;
using ApprovalTests.Reporters;
using Coulda.Test;
using FubuCore;
using FubuCore.Reflection;
using HtmlTags;
using HtmlTags.Conventions;
using Xunit;

namespace SharpViews.Tests
{

    public class GridContainer : ContentTag
    {
        private List<GridRow> _rows;

        public GridContainer(int rows)
        {
            _rows = new List<GridRow>();
        }

        public void Configure(Action<RowBuilder> config)
        {
            var builder = new RowBuilder();
            config(builder);
            builder.UpdateRows(_rows);
        }

        public ColumnPartView this[int row, int col]
        {
            get { return _rows[row][col]; }
            set { _rows[row][col] = value; }
        }

        protected override void Content()
        {
          
            foreach (var row in _rows)
            {
                append(row);
            }
        }
    }

    public class GridRow : ContentTag
    {
        private List<ColumnPartView> _columns;

        public GridRow(int columnCount = 1,Action<ColumnSizeBuilder> sizes = null)
        {
            _columns = new List<ColumnPartView>();
            for (int i = 0; i < columnCount; i++)
            {
                _columns.Add(new ColumnPartView());
            }

            if (sizes != null)
            {
                var sizeBuilder = new ColumnSizeBuilder(columnCount);
                sizes(sizeBuilder);
                sizeBuilder.UpdateColumns(_columns);

            }
        }

        public void UpdateColumns(ColumnSizeBuilder builder)
        {
            builder.UpdateColumns(_columns);
        }

        public ColumnPartView this[int index]
        {
            get { return _columns[index]; }
            set { _columns[index] = value; }
        }

        public ColumnPartView Column(int idx)
        {
            return this[idx];
        }

        protected override void Content()
        {
            row(() =>
            {
                foreach (var column in _columns)
                {
                    append(column);
                }

            });
        }
    }

    public enum DeviceSize
    {
        XS = 0,
        SM = 1,
        MD = 2,
        LG = 3,
    }

    public struct ColumnSettings
    {
        public int Width { get; private set; }
        public int Offset { get; private set; }
        public  DeviceSize Profile { get;private set; }

        public ColumnSettings(int width, int offset,DeviceSize ds) : this()
        {
            Width = width;
            Offset = offset;
            Profile = ds;

        }

        public ColumnSettings WithWidth(int width)
        {
            return new ColumnSettings(width,Offset,Profile);
        }

        public ColumnSettings WithOffset(int offset)
        {
            return new ColumnSettings(Width, offset,Profile);
        }

        public string WidthClass()
        {
            if (HasWidth())
            {
                return String.Format("col-{0}-{1}", Profile.ToString().ToLowerInvariant(), Width);
            }
            return "";
        }

        private bool HasWidth()
        {
            return Width > 0;
        }

        public string OffsetClass()
        {
            if (HasOffset())
            {
                return String.Format("col-{0}-offset-{1}", Profile.ToString().ToLowerInvariant(), Offset);
            }

            return "";
        }

        private bool HasOffset()
        {
            return Offset > 0;
        }

        public IEnumerable<string> CssClasses()
        {
            if (HasWidth())
            {
                yield return WidthClass();
            }

            if (HasOffset())
            {
                yield return OffsetClass();
            }
        } 
    }

    public class RowBuilder
    {
        private List<ColumnSizeBuilder> _rows;

        public RowBuilder()
        {
            _rows = new List<ColumnSizeBuilder>();
        }

        public ColumnSizeBuilder Row(int num)
        {
            if (num >= _rows.Count)
            {
                _rows.Add(new ColumnSizeBuilder(0));
            }

            return _rows[num];
        }


        public void UpdateRows(List<GridRow> rows)
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                if (i >= rows.Count)
                {
                    rows.Add(new GridRow());
                }

                var row = rows[i];
                if (i < _rows.Count)
                {
                    var rowSettings = _rows[i];
                    row.UpdateColumns(rowSettings);
                }
            }
        }
    }

    public class ColumnSizeBuilder
    {
        private List<ColumnSettings[]> _settings;
        private int _currentIndex;

        public ColumnSizeBuilder(int columCount)
        {
            _settings = new List<ColumnSettings[]>();
            _currentIndex = 0;
        }

        private ColumnSettings[] DefaultSettings()
        {
            return new[]
            {
                new ColumnSettings(0, 0, DeviceSize.XS),
                new ColumnSettings(0, 0, DeviceSize.SM),
                new ColumnSettings(0, 0, DeviceSize.MD),
                new ColumnSettings(0, 0, DeviceSize.LG),
            };
        }
        public ColumnSizeBuilder Column(int num)
        {
            _currentIndex = num;
            
            while(_currentIndex >= _settings.Count)
            {
               _settings.Add(DefaultSettings());
            }

            return this;
        }

        public ColumnSizeBuilder Width(int width)
        {
            return Width(DeviceSize.MD, width);
        }

        public ColumnSizeBuilder Width(DeviceSize profile,int width)
        {
            _settings[_currentIndex][(int)profile] = _settings[_currentIndex][(int)profile].WithWidth(width);
            return this;
        }

        public ColumnSizeBuilder Offset(int offset)
        {
            return Offset(DeviceSize.MD, offset);
        }

        public ColumnSizeBuilder Offset(DeviceSize profile, int offset)
        {
            _settings[_currentIndex][(int)profile] = _settings[_currentIndex][(int)profile].WithOffset(offset);
            return this;
        }

        public void UpdateColumns(List<ColumnPartView> columns)
        {
            for (int i = 0; i < _settings.Count; i++)
            {
                while (i >= columns.Count){
                    columns.Add(new ColumnPartView());
                }
            
                var column = columns[i];

                var xssettings = _settings[i][(int)DeviceSize.XS];
                var smsettings = _settings[i][(int)DeviceSize.SM];
                var mdsettings = _settings[i][(int)DeviceSize.MD];
                var lgsettings = _settings[i][(int)DeviceSize.LG];

                column.SetSizes(new[] {xssettings,smsettings,mdsettings,lgsettings});
                
            }
        }
    }
    public class ColumnPartView : ContentTag
    {
        public List<ITagSource> _children;
        private HtmlTag _tag;
        private ColumnSettings[] _sizes;

        public ColumnPartView()
        {
            _children = new List<ITagSource>();
            _tag = new DivTag();
            _sizes = new ColumnSettings[0];

        }

        public void SetSizes(IEnumerable<ColumnSettings> settings)
        {
            _sizes = settings.ToArray();
        }

        public void Append(ITagSource htmlTag)
        {
            _children.Add(htmlTag);
        }

        public static ColumnPartView operator +(ColumnPartView cpv, ITagSource htmlTag)
        {
            cpv.Append(htmlTag);
            return cpv;
        }

        protected override void Content()
        {
            var sizeCss = _sizes.SelectMany(x => x.CssClasses()).ToArray();
            _tag.AddClasses(sizeCss);
            foreach (var c in _children)
            {
                _tag.Append(c);
            }
            append(_tag);
        }

        
    }

    public class RegisterLinkView : ContentTag
    {
        private string _registerUrl;

        public RegisterLinkView(string registerUrl)
        {
            _registerUrl = registerUrl;
        }

        protected override void Content()
        {
            div(() =>
            {
                a(href: _registerUrl, classes: new[] { "btn", "btn-success", "btn-block", "btn-lg" },
                    text: "Sign Up");
                p("You will need your most recent invoice").AddClass("help-block");

            }).Style("margin-top", "20px");
        }
    }

    public class RawHtmlView : ContentTag
    {
        private string _htmlText;

        public RawHtmlView(string htmlText)
        {
            _htmlText = htmlText;
        }

        protected override void Content()
        {
            div(() =>
            {
                html(_htmlText);

            });
        }
    }
    public class LoginPageView : ContentTag
    {
        public bool EmailNotConfirmed { get; set; }

        public bool CanRegister { get; set; }

        public string ResendEmailUrl { get; set; }

        public string RecoverPasswordUrl { get; set; }

        public string LoginUrl { get; set; }

        public string RegisterUrl { get; set; }

        public string SslLogoContent { get; set; }

        protected override void Content()
        {
           

            tag("section",id:"login_main", children:() =>
            {
                if (EmailNotConfirmed)
                {
                    var resendView = new ResendView();
                    resendView.Url = ResendEmailUrl;
                    append(resendView);
                }

                var grid = new GridContainer(0);

                grid.Configure(x =>
                {
                    x.Row(0)
                        .Column(0).Width(4)
                        .Column(1).Offset(1).Width(3);
                });
                grid[0, 0] += new LoginForm();

                if (CanRegister)
                {
                    grid[0, 1] += new RegisterLinkView(RegisterUrl);
                }

                grid[0, 1] += new RawHtmlView(SslLogoContent);

                append(grid);
            });
        }
    }

    public class ResendView : ContentTag
    {
        protected override void Content()
        {
            row(() =>
            {
                col(mdOff: 2,md: 6,children: () =>
                {
                    div(children: () =>
                    {
                        strong("Important!");
                        p(@"You must confirm your email address before you can sign in to
                            this site.");
                        a(href:Url,text:"Resend Email",classes:new[] {"btn","btn-primary"});
                    }, classes: "alert alert-warning");
                });
            });
        }

        public string Url { get; set; }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }

    }

    public class LoginForm : FormView
    {
        public LoginForm()
        {
            this.Action = "/login";
            Method = "POST";
            AutoComplete = "off";
            Classes = new[] {"form-stacked"};

            DefineFields<LoginModel>(f =>
            {
                f.AddField(x => x.Username);
                f.AddField(x => x.Password);
            });

            SubmitButton
                .AddClass("btn-primary")
                .Text("Login");

            SecondaryLinks.Add(new HtmlTag("a").Text("Forgot Password?").Attr("href","/recover"));
        }
    }
    public class FormField
    {
        private readonly Accessor _accessor;

        public FormField(Accessor accessor)
        {
            _accessor = accessor;
        }

        public string LabelText
        {
            get { return _accessor.FieldName.SplitCamelCase(); }
        }

        public string Name
        {
            get
            {
                var name = String.Join(".", _accessor.PropertyNames);
                return Char.ToLowerInvariant(name[0]) + name.Substring(1);
            }
        }

        public string Id
        {
            get { return Name.Replace('.', '_'); }
        }

        public void BuildInput(ContentTag ct)
        {
            ct.tag("input",id:Id,name:Name,classes:new[]{"form-control"});
        }

        public void BuildLabel(ContentTag formView)
        {
            formView.label(@for:Id,text:LabelText,classes:new[] {"control-label"});
        }
    }

    public class FieldBuilder<MODEL>
    {
        private readonly List<FormField> _fields;

        public FieldBuilder(List<FormField> fields)
        {
            _fields = fields;
        }
        public FormField AddField(Expression<Func<MODEL,object>> expr)
        {
            var accessor = ReflectionHelper.GetAccessor(expr);
            var field = new FormField(accessor);
            _fields.Add(field);
            return field;
        }
    }

    public class FormView : ContentTag
    {
        private List<FormField> _fields;
        private HtmlTag _submitButton;
        private List<HtmlTag> _secondaryLinks; 

        public FormView()
        {
            _fields = new List<FormField>();
            _submitButton = new HtmlTag("button")
                              .Attr("type","submit")
                              .AddClass("btn")
                              .Text("Submit");

            _secondaryLinks = new List<HtmlTag>();
        }

        public string Action { get; set; }
        public string Method { get; set; }

        public string  AutoComplete { get; set; }

        public IEnumerable<string> Classes { get; set; }

        public HtmlTag SubmitButton
        {
            get { return _submitButton; }
        }

        public List<HtmlTag> SecondaryLinks { get { return _secondaryLinks; } }
        public void DefineFields<T>(Action<FieldBuilder<T>> fieldDefinitions)
        {
            var builder = new FieldBuilder<T>(_fields);
            fieldDefinitions(builder);

        }

        protected override void Content()
        {
            form(action:Action,method:Method,autocomplete:AutoComplete,classes: Classes,children: () =>
            {
                foreach (var field in _fields)
                {
                    div(classes:"form-group",children:() =>
                    {
                        field.BuildLabel(this);
                        field.BuildInput(this);
                    });
                }

                div(classes: "form-group",children:() =>
                {
                    if (SecondaryLinks.Count > 0)
                    {
                        div(() =>
                        {
                            append(_submitButton);
                            foreach (var link in SecondaryLinks)
                            {
                                append(link);
                            }
                        });
                    }
                    else
                    {
                        append(_submitButton);
                    }
                });
            });
        }
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

        public void form(IEnumerable<string> classes = null, string action = null, string method = null,string autocomplete = null,
            Action children = null)
        {
            tag("form",classes:classes,action:action,method:method,autocomplete:autocomplete,children:children);
        }

        public void label(string text = null,string @for = null,IEnumerable<string> classes = null, Action children = null)
        {
            tag("label",@for:@for,text:text,classes:classes,children:children);
        }

        public void append(ITagSource htmlTag)
        {
            _currentScope.Peek().Append(htmlTag);
        }

        public void html(string htmlContent)
        {
            _currentScope.Peek().AppendHtml(htmlContent);
        }

        public HtmlTag tag(string tagName,string name = null,string id = null,string type = null,string @for = null,string autocomplete = null, 
                        IEnumerable<string> classes = null, string action = null, string method = null,
                        string text = null, Action children = null)
        {
            var theTag = new HtmlTag(tagName);

            if (type != null)
            {
                theTag.Attr("type", type);
            }

            if (id != null)
            {
                theTag.Id(id);
            }

            if (name != null)
            {
                theTag.Attr("name", name);
            }

            if (@for != null)
            {
                theTag.Attr("for", @for);
            }

            if (classes != null)
            {
                theTag.AddClasses(classes);
            }

            if (action != null)
            {
                theTag.Attr("action", action);
            }

            if (method != null)
            {
                theTag.Attr("method", method);
            }

            if (autocomplete != null)
            {
                theTag.Attr("autocomplete", autocomplete);
            }
            
            if (text != null)
            {
                theTag.Text(text);
            }

            AddChildContent(theTag,children);

            return theTag;
        }

        public HtmlTag row(Action children)
        {
            return div(children: children, classes: "row");
        }

        private void AddChildContent(HtmlTag tag, Action children)
        {
            _currentScope.Peek().Append(tag);
            if (children != null)
            {
                _currentScope.Push(tag);
                children();
                _currentScope.Pop();
            }
        }

        public HtmlTag col(int? md = null, int? xs = null, int? sm = null, int? lg = null, int? mdOff = null,
            int? smOff = null, int? xsOff = null, int? lgOff = null, Action children =null)
        {
            var theTag = new HtmlTag("div");
            if (md.HasValue)
            {
                theTag.AddClass("col-md-" + md.Value);
            }
            if (xs.HasValue)
            {
                theTag.AddClass("col-xs-" + xs.Value);
            }
            if (sm.HasValue)
            {
                theTag.AddClass("col-sm-" + sm.Value);
            }
            if (lg.HasValue)
            {
                theTag.AddClass("col-lg-" + lg.Value);
            }
            if (mdOff.HasValue)
            {
                theTag.AddClass("col-md-offset-" + mdOff.Value);
            }
            if (smOff.HasValue)
            {
                theTag.AddClass("col-sm-offset-" + smOff.Value);
            }
            if (xsOff.HasValue)
            {
                theTag.AddClass("col-xs-offset-" + xsOff.Value);
            }
            if (lgOff.HasValue)
            {
                theTag.AddClass("col-lg-offset-" + lgOff.Value);
            }

            AddChildContent(theTag, children);
            return theTag;
        }

        public HtmlTag strong(string txt)
        {
            var theTag =  new HtmlTag("strong").Text(txt);
            _currentScope.Peek().Append(theTag);
            return theTag;
        }

        public HtmlTag p(string text)
        {
            var theTag =  new HtmlTag("p").Text(text);
            _currentScope.Peek().Append(theTag);
            return theTag;
        }

        public HtmlTag a(string href, string text, string[] classes)
        {
            var theTag  = new HtmlTag("a").Attr("href", href).Text(text).AddClasses(classes);
            _currentScope.Peek().Append(theTag);
            return theTag;

        }

        public HtmlTag div(Action children = null, string classes = null)
        {
            var theTag = new HtmlTag("div");
            if (classes != null)
            {
                var classesArr = classes.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var c in classesArr)
                {
                    theTag.AddClass(c);
                }
            }

            AddChildContent(theTag,children);
            return theTag;
        }

        public IEnumerable<HtmlTag> AllTags()
        {
            Content();
            yield return _rootTag;
        }

        protected virtual void Content()
        {
            
        }

        public string PrettyPrintHtml()
        {
            Content();
            return _rootTag.ToPrettyString();
        }
    }
    public class PageViewTests : CouldaBase
    {
        [Fact]
        [UseReporter(typeof(ApprovalTests.Reporters.VisualStudioReporter))]
        public void Verify_login_view_html()
        {
            var view = new LoginPageView()
            {
                CanRegister = true,
                LoginUrl = "/login",
                RegisterUrl = "/register",
                ResendEmailUrl = "/resend",
                EmailNotConfirmed = true,
                RecoverPasswordUrl = "/recover",
                SslLogoContent = "<h1>YOUR LOGO</h1>"
            };
            
            Approvals.Verify(view.PrettyPrintHtml());
        }

        
    }
}