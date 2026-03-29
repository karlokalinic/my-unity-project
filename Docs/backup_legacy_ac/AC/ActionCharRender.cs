using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionCharRender : Action
	{
		public int parameterID = -1;

		public int constantID;

		public bool isPlayer;

		public Char _char;

		protected Char runtimeChar;

		public RenderLock renderLock_sorting;

		public SortingMapType mapType;

		public int sortingOrder;

		public int sortingOrderParameterID = -1;

		public string sortingLayer;

		public int sortingLayerParameterID = -1;

		public RenderLock renderLock_scale;

		public int scale;

		public RenderLock renderLock_direction;

		public CharDirection direction;

		public RenderLock renderLock_sortingMap;

		public SortingMap sortingMap;

		public int sortingMapConstantID;

		public int sortingMapParameterID = -1;

		protected SortingMap runtimeSortingMap;

		public SortingMap RuntimeSortingMap
		{
			get
			{
				return runtimeSortingMap;
			}
		}

		public ActionCharRender()
		{
			isDisplayed = true;
			category = ActionCategory.Character;
			title = "Change rendering";
			description = "Overrides a Character's scale, sorting order, sprite direction or Sorting Map. This is intended mainly for 2D games.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeChar = AssignFile(parameters, parameterID, constantID, _char);
			if (isPlayer)
			{
				runtimeChar = KickStarter.player;
			}
			sortingOrder = AssignInteger(parameters, sortingOrderParameterID, sortingOrder);
			sortingLayer = AssignString(parameters, sortingLayerParameterID, sortingLayer);
			runtimeSortingMap = AssignFile(parameters, sortingMapParameterID, sortingMapConstantID, sortingMap);
		}

		public override float Run()
		{
			if (runtimeChar != null)
			{
				if (renderLock_sorting == RenderLock.Set)
				{
					if (mapType == SortingMapType.OrderInLayer)
					{
						runtimeChar.SetSorting(sortingOrder);
					}
					else if (mapType == SortingMapType.SortingLayer)
					{
						runtimeChar.SetSorting(sortingLayer);
					}
				}
				else if (renderLock_sorting == RenderLock.Release)
				{
					runtimeChar.ReleaseSorting();
				}
				if (runtimeChar.GetAnimEngine() != null)
				{
					runtimeChar.GetAnimEngine().ActionCharRenderRun(this);
				}
			}
			return 0f;
		}

		public static ActionCharRender CreateNew_Mecanim(Char characterToAffect, RenderLock sortingLock, int newSortingOrder, RenderLock scaleLock, int newScale)
		{
			ActionCharRender actionCharRender = ScriptableObject.CreateInstance<ActionCharRender>();
			actionCharRender._char = characterToAffect;
			actionCharRender.renderLock_sorting = sortingLock;
			actionCharRender.mapType = SortingMapType.OrderInLayer;
			actionCharRender.sortingOrder = newSortingOrder;
			actionCharRender.renderLock_scale = scaleLock;
			actionCharRender.scale = newScale;
			return actionCharRender;
		}

		public static ActionCharRender CreateNew_Mecanim(Char characterToAffect, RenderLock sortingLock, string newSortingLayer, RenderLock scaleLock, int newScale)
		{
			ActionCharRender actionCharRender = ScriptableObject.CreateInstance<ActionCharRender>();
			actionCharRender._char = characterToAffect;
			actionCharRender.renderLock_sorting = sortingLock;
			actionCharRender.mapType = SortingMapType.SortingLayer;
			actionCharRender.sortingLayer = newSortingLayer;
			actionCharRender.renderLock_scale = scaleLock;
			actionCharRender.scale = newScale;
			return actionCharRender;
		}

		public static ActionCharRender CreateNew_Sprites(Char characterToAffect, RenderLock sortingLock, int newSortingOrder, RenderLock scaleLock, int newScale, RenderLock directionLock, CharDirection newDirection, RenderLock sortingMapLock, SortingMap newSortingMap)
		{
			ActionCharRender actionCharRender = ScriptableObject.CreateInstance<ActionCharRender>();
			actionCharRender._char = characterToAffect;
			actionCharRender.renderLock_sorting = sortingLock;
			actionCharRender.mapType = SortingMapType.OrderInLayer;
			actionCharRender.sortingOrder = newSortingOrder;
			actionCharRender.renderLock_scale = scaleLock;
			actionCharRender.scale = newScale;
			actionCharRender.renderLock_direction = directionLock;
			actionCharRender.direction = newDirection;
			actionCharRender.renderLock_sortingMap = sortingLock;
			actionCharRender.sortingMap = newSortingMap;
			return actionCharRender;
		}

		public static ActionCharRender CreateNew_Sprites(Char characterToAffect, RenderLock sortingLock, string newSortingLayer, RenderLock scaleLock, int newScale, RenderLock directionLock, CharDirection newDirection, RenderLock sortingMapLock, SortingMap newSortingMap)
		{
			ActionCharRender actionCharRender = ScriptableObject.CreateInstance<ActionCharRender>();
			actionCharRender._char = characterToAffect;
			actionCharRender.renderLock_sorting = sortingLock;
			actionCharRender.mapType = SortingMapType.SortingLayer;
			actionCharRender.sortingLayer = newSortingLayer;
			actionCharRender.renderLock_scale = scaleLock;
			actionCharRender.scale = newScale;
			actionCharRender.renderLock_direction = directionLock;
			actionCharRender.direction = newDirection;
			actionCharRender.renderLock_sortingMap = sortingLock;
			actionCharRender.sortingMap = newSortingMap;
			return actionCharRender;
		}
	}
}
