using HeBianGu.Base.WpfBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeBianGu.WebProxy.Titanium
{
    public interface ITrigger
    {
        bool IsVisible { get; set; }
        void Invoke(SessionListItem session);
    }

    public abstract class TriggerBase : DisplayerViewModelBase, ITrigger
    {
        private bool _isVisible = true;
        /// <summary> 说明  </summary>
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                RaisePropertyChanged();
            }
        }

        public abstract void Invoke(SessionListItem session);
    }

    public class PredicateTrigger : TriggerBase
    {
        public readonly Action<SessionListItem> _action;

        public PredicateTrigger(Action<SessionListItem> action)
        {
            _action = action;
        }
        public override void Invoke(SessionListItem session)
        {
            this._action?.Invoke(session);
        }
    }

    [Displayer(Name = "测试规则")]
    public class TestTrigger : TriggerBase
    {
        public override void Invoke(SessionListItem session)
        {
            if (session.Host == "www.baidu.com")
            {
                MessageProxy.Notify.ShowInfo("你操作了百度：进程" + session.Process);
            }
        }
    }
}
