#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.Collections;
using System.Windows.Controls;
using MediaPortal.GUI.Library;

namespace System.Windows
{
  public sealed class EventManager
  {
    #region Constructors

    private EventManager() {}

    #endregion Constructors

    #region Methods

    public static RoutedEvent GetRoutedEventFromName(string name, Type ownerType)
    {
      // MPSPECIFIC
      //if (ownerType == typeof (GUIWindow))
      //{
      //  ownerType = typeof (Page);
      //}

      return (RoutedEvent)_registeredRoutedEvents[ownerType + name];
    }

    public static RoutedEvent[] GetRoutedEvents()
    {
      throw new NotImplementedException();
    }

    public static RoutedEvent[] GetRoutedEventsForOwner(Type ownerType)
    {
      throw new NotImplementedException();
    }

    public static void RegisterClassHandler(Type classType, RoutedEvent routedEvent, Delegate handler)
    {
//			_registeredClassHandlers[classType].AddHandler(routedEvent, handler, true);
    }

    public static void RegisterClassHandler(Type classType, RoutedEvent routedEvent, Delegate handler,
                                            bool handledEventsToo)
    {
//			_registeredClassHandlers[classType].AddHandler(routedEvent, handler, handledEventsToo);
    }

    public static RoutedEvent RegisterRoutedEvent(string name, RoutingStrategy routingStrategy, Type handlerType,
                                                  Type ownerType)
    {
      RoutedEvent routedEvent = new RoutedEvent(name, routingStrategy, handlerType, ownerType);

      _registeredRoutedEvents[ownerType + name] = routedEvent;

      return routedEvent;
    }

    #endregion Methods

    #region Fields

    private static Hashtable _registeredRoutedEvents = new Hashtable();
    private static RoutedEventHandlerInfoStore _registeredClassHandlers = new RoutedEventHandlerInfoStore();

    #endregion Fields
  }
}