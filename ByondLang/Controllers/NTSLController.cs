using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ByondLang.Language;
using ByondLang.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace ByondLang.Controllers
{
    [ApiController]
    public class NTSLController : ControllerBase
    {
        NTSLService _service;
        ILogger _log;
        public NTSLController(NTSLService service, ILogger<NTSLController> logger)
        {
            _service = service;
            _log = logger;
        }


        [HttpGet("/clear")]
        public int Clear()
        {
            _service.Reset();
            return 1;
        }

        [HttpGet("/new_program")]
        public int NewProgram([FromQuery] string code = "", [FromQuery(Name = "ref")] string computerRef = "")
        {
            try
            {
                return _service.NewProgram(code, computerRef);
            }
            catch (Exception e)
            {
                _log.LogError(e, "New program failed with exception.");
                return 0;
            }
        }

        [HttpGet("/execute")]
        public int Execute([FromQuery] int id, [FromQuery] int cycles = 0)
        {
            try
            {
                _service.Execute(id, cycles);
                return 1;
            }
            catch (Exception e)
            {
                _log.LogError(e, "Execute failed with exception.");
                return 0;
            }
        }

        [HttpGet("/get_buffer")]
        public string GetBuffer([FromQuery] int id, [FromQuery] int cycles = 0)
        {
            try
            {
                return _service.GetTerminalBuffer(id);
            }
            catch (Exception e)
            {
                _log.LogError(e, "GetBuffer failed with exception.");
                return "Unknown Error.";
            }
        }

        [HttpGet("/remove")]
        public int Remove([FromQuery] int id, [FromQuery] string signal_ref, [FromQuery] string encSignal)
        {
            var signal = HttpUtility.ParseQueryString(encSignal);
            try
            {
                _service.ProcessMessage(id, signal_ref, signal);
                return 1;
            }
            catch (Exception e)
            {
                _log.LogError(e, "Remove failed with exception.");
                return 0;
            }
        }

        [HttpGet("/get_signal")]
        public Signal? GetSignal([FromQuery] int id)
        {
            try
            {
                return _service.GetSignal(id);
            }
            catch (Exception e)
            {
                _log.LogError(e, "GetSignal failed with exception.");
                return null;
            }
        }

        [HttpGet("/subspace_receive")]
        public int SubspaceReceive([FromQuery] string channel, [FromQuery] string type, [FromQuery] string data)
        {
            try
            {
                _service.SubspaceReceive(channel, type, data);
                return 1;
            }
            catch (Exception e)
            {
                _log.LogError(e, "SubspaceReceive failed with exception.");
                return 0;
            }
        }


        [HttpGet("/subspace_transmit")]
        public string SubspaceTransmit()
        {
            try
            {
                return _service.GetSubspaceMessageToSend();
            }
            catch (Exception e)
            {
                _log.LogError(e, "SubspaceTransmit failed with exception.");
                return "0";
            }
        }

        [HttpGet("/topic")]
        public int TopicCall([FromQuery] int id, [FromQuery] string topic = "")
        {
            try
            {
                _service.HandleTopic(id, topic);
                return 1;
            }
            catch (Exception e)
            {
                _log.LogError(e, "TopicCall failed with exception.");
                return 0;
            }
        }
    }
}
