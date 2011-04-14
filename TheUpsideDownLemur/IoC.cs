/*This file is part of Sc2ReplayMonkey
    Sc2ReplayMonkey is a starcraft II replay analyzer tool built upon SC2PArserApe  
 
    Copyright (C) 2011  Louis Kottmann louis.kottmann@gmail.com

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheUpsideDownLemur
{
    public static class IoC
    {
        static Dictionary<Type, object> _registeredTypes = new Dictionary<Type, object>();

        public static void AddMonkey<T>(T toRegister)
        {
            _registeredTypes.Add(typeof(T), toRegister);
        }

        public static T FindMonkey<T>()
        {
            return (T)_registeredTypes[typeof(T)];
        }
    }
}
