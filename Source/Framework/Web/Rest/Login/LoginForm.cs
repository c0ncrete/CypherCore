﻿// Copyright (c) CypherCore <http://github.com/CypherCore> All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Framework.Web.Rest.Login
{
    public class LoginForm
    {
        [JsonPropertyName("platform_id")]
        public string PlatformId { get; set; }

        [JsonPropertyName("program_id")]
        public string ProgramId { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("inputs")]
        public List<FormInputValue> Inputs { get; set; } = new List<FormInputValue>();
    }
}
