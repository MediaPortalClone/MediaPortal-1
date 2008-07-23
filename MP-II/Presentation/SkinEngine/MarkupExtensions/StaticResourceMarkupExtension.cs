#region Copyright (C) 2007-2008 Team MediaPortal

/*
    Copyright (C) 2007-2008 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal II

    MediaPortal II is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal II is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal II.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using Presentation.SkinEngine.Xaml.Exceptions;
using Presentation.SkinEngine.SkinManagement;
using Presentation.SkinEngine.Xaml;
using Presentation.SkinEngine.Xaml.Interfaces;
using Presentation.SkinEngine.Controls.Visuals;
using Presentation.SkinEngine.MpfElements.Resources;

namespace Presentation.SkinEngine.MarkupExtensions
{
  /// <summary>
  /// Implements the MPF StaticResource markup extension.
  /// </summary>
  public class StaticResourceMarkupExtension: StaticResourceBase, IEvaluableMarkupExtension
  {
    #region Protected fields

    protected string _resourceKey;

    #endregion

    public StaticResourceMarkupExtension() { }

    public StaticResourceMarkupExtension(string resourceKey)
    {
      _resourceKey = resourceKey;
    }

    #region Properties

    public string ResourceKey
    {
      get { return _resourceKey; }
      set { _resourceKey = value; }
    }

    #endregion

    #region IEvaluableMarkupExtension implementation

    object IEvaluableMarkupExtension.Evaluate(IParserContext context)
    {
      object result = FindResourceInParserContext(_resourceKey, context);
      if (result == null)
        result = FindResourceInTheme(_resourceKey);

      if (result == null)
        throw new XamlBindingException("StaticResourceMarkupExtension: Resource '{0}' not found", _resourceKey);
      return result;
    }

    #endregion
  }
}
