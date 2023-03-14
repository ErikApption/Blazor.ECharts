using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Blazor.ECharts.Options;
using Blazor.ECharts.Options.Enum;
using System.Xml.Linq;

namespace Blazor.ECharts
{
    // This class provides an example of how JavaScript functionality can be wrapped
    // in a .NET class for easy consumption. The associated JavaScript module is
    // loaded on demand when first needed.
    //
    // This class can be registered as scoped DI service and then injected into Blazor
    // components for use.

    public class JsInterop : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;

        public JsInterop(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
               "import", "./_content/Blazor.ECharts/core.js").AsTask());
        }

        public async ValueTask<string> Prompt(string message)
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<string>("showPrompt", message);
        }

        /// <summary>
        /// ��ʼ��Echarts
        /// </summary>
        /// <param name="id">ECharts����ID</param>
        /// <param name="theme">����</param>
        /// <returns></returns>
        public async ValueTask<IJSObjectReference> InitChart(string id, string theme = "light")
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "echarts�ؼ�id����Ϊ��");
            var module = await moduleTask.Value;
            return await module.InvokeAsync<IJSObjectReference>("echartsFunctions.initChart", id, theme);
        }
        public async Task RegisterMap(string name, string svg)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("echartsFunctions.registerMap", name, svg);
        }
        /// <summary>
        /// ����Echarts����
        /// </summary>
        /// <param name="id">ECharts����ID</param>
        /// <param name="theme">����</param>
        /// <param name="option">����</param>
        /// <returns></returns>
        public async Task SetupChart<T>(string id, string theme, EChartsOption<T> option, bool notMerge = false)
        {
            await SetupChart(id, theme, option.ToString(), notMerge);
        }

        /// <summary>
        /// ����Echarts����
        /// </summary>
        /// <param name="id">ECharts����ID</param>
        /// <param name="theme">����</param>
        /// <param name="option">����</param>
        /// <returns></returns>
        public async Task SetupChart(string id, string theme, string option, bool notMerge = false)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "echarts�ؼ�id����Ϊ��");
            if (option == null) throw new ArgumentNullException(nameof(option), "echarts��������Ϊ��");
            if (string.IsNullOrWhiteSpace(theme)) theme = "light";
            var module = await moduleTask.Value;
            try
            {
                await module.InvokeVoidAsync("echartsFunctions.setupChart", id, theme, option, notMerge);
            }
            catch
            {
                Console.WriteLine("id:" + id);
                Console.WriteLine("theme:" + theme);
                Console.WriteLine("option:" + option);
                Console.WriteLine("notMerge:" + notMerge);
            }
        }

        /// <summary>
        /// ����Ӧ
        /// </summary>
        /// <param name="id">ECharts����ID</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task Resize(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "echarts�ؼ�id����Ϊ��");
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("echartsFunctions.resize", id);
        }

        public async Task ChartOn(string id, EventType eventType, DotNetObjectReference<EventInvokeHelper> objectReference)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("echartsFunctions.on", id, eventType.ToString(), objectReference);
        }
#nullable enable
        /// <summary>
        /// ͸������
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async ValueTask InvokeVoidAsync(string identifier, params object?[] args)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync(identifier, args);
        }
#nullable disable

        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }

        public async Task ChartShowLoading(string id, string option)
        {
			var module = await moduleTask.Value;
			await module.InvokeVoidAsync("echartsFunctions.showLoading", id, option);
		}

		public async Task ChartHideLoading(string id)
		{
			var module = await moduleTask.Value;
			await module.InvokeVoidAsync("echartsFunctions.hideLoading", id);
		}
	}
}
