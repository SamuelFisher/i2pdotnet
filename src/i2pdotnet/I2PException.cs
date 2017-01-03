// This file is part of i2pdotnet.
// Copyright (c) 2016
//  
// i2pdotnet is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as
// published by the Free Software Foundation, version 3.
//  
// i2pdotnet is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//  
// You should have received a copy of the GNU Lesser General Public License
// along with i2pdotnet.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace I2PNet
{
    public class I2PException : Exception
    {
        public I2PException(string message, string errorCode, string rawMessage = null)
            : base(message)
        {
            ErrorCode = errorCode;
            RawMessage = rawMessage;
        }

        public string ErrorCode { get; }

        public string RawMessage { get; }
    }
}
