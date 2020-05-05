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

namespace ByondLang.Controllers
{
    [ApiController]
    [Produces("text/plain")]
    public class NTSLController : ControllerBase
    {
        NTSLService _service;
        public NTSLController(NTSLService service)
        {
            _service = service;
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
            var id = _service.NewProgram(code, computerRef);
            return id;
        }

        [HttpGet("/execute")]
        public int Execute([FromQuery] int id, [FromQuery] int cycles = 0)
        {
            try
            {
                _service.Execute(id, cycles);
                return 1;
            }
            catch (Exception)
            {
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
            catch (Exception)
            {
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
            catch (Exception)
            {
                return 0;
            }
        }

        [HttpGet("/get_signals")]
        [Produces("application/json")]
        public Signal[] GetSignals([FromQuery] int id)
        {
            try
            {
                return _service.GetSignals(id);
            }
            catch (Exception)
            {
                return null;
            }
        }

        [HttpGet("/subspace_receive")]
        public int SubspaceReceive([FromQuery] int id, [FromQuery] string channel, [FromQuery] string type, [FromQuery] string data)
        {
            try
            {
                _service.SubspaceReceive(id, channel, type, data);
                return 1;
            }
            catch (Exception)
            {
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
            catch (Exception)
            {
                return null;
            }
        }

        [HttpGet("/topic")]
        public int TopicCall([FromQuery] int id, [FromQuery] string topic)
        {
            try
            {
                _service.HandleTopic(id, topic);
                return 1;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
