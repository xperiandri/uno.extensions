using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace nVentive.Umbrella.Collections.Tracking
{
	partial class CollectionTracker
	{
		private class ChangesBuffer
		{
			/*
			 * We generate 4 kinds of changes:
			 * - Add: are based on the RESULT index and impacts the RESULT index
			 * - Move: are based on the OLD index and impacts the RESULT index
			 * - Remove: are based on the OLD index and impacts the OLD index
			 * - Replace: are based on the OLD index
			 * 	
			 * 	As we always notify the Replace BEFORE other notifications and it does not impacts the RESULT index, 
			 * 	we can buffer them in parallel with add or move (works on OLD index - That's why we have dedicated buffers for the Replace operations).
			 * 	But if something impact the OLD index (i.e. Remove) we have to execute them before in order to re-sync indexes.
			 * 	
			 */

			private readonly int _oldItemsCount, _newItemsCount;
			private readonly int _eventArgsOffset;

			private readonly Head _head;
			private IChange _tail;
			private _Same? _sameHead;
			private _Replace? _replaceHead;

			public ChangesBuffer(int oldItemsCount, int newItemsCount, int eventArgsOffset)
			{
				_oldItemsCount = oldItemsCount;
				_newItemsCount = newItemsCount;
				_eventArgsOffset = eventArgsOffset;

				_tail = _head = new Head();
			}

			private IChange Tail
			{
				get => _tail;
				set => _tail = _tail.Next = value;
			}

			public void Append(IChange change) => Tail = change;

			/// <summary>
			/// Flush the buffers and retrieve the full list of changes
			/// </summary>
			/// <returns></returns>
			public ChangeBase? GetChanges()
			{
				// Start with the '_replace'
				IChange? head = _replaceHead, tail = _replaceHead;

				// If we have some '_same', append it to the tail
				if (_sameHead is not null)
				{
					if (head is null)
					{
						head = tail = _sameHead;
					}
					else
					{
						SeekToTail(ref tail);
						tail!.Next = _sameHead;
					}

					
				}

				// Then append all other changes (Add / Move / Remove)
				// Note: as the '_head' is initialized with a 'Head', we can bypass it
				if (head is null)
				{
					head = _head.Next;
				}
				else
				{
					SeekToTail(ref tail);
					tail!.Next = _head.Next;
				}

				return head as ChangeBase;

				static void SeekToTail(ref IChange? tail)
				{
					while (tail?.Next is not null)
					{
						tail = tail.Next;
					}
				}
			}

			public void Update(object oldItem, object newItem, int index)
			{
				// As they don't impact the 'result' index, replace-instance are buffered separately and will be inserted at the top of the changes collection.
				// Note: We use a single '_Same' node for all updates

				UpdateOrReplace(ref _sameHead, oldItem, newItem, index, (i, o) => new _Same(i, o)); ;
			}

			public void Replace(object oldItem, object newItem, int index)
			{
				// As they don't impact the 'result' index, replaces are buffered separately and will be inserted at the top of the changes collection.

				UpdateOrReplace(ref _replaceHead, oldItem, newItem, index, (i, o) => new _Replace(i, o));
			}



			public void Add(object item, int at, int max)
			{
				if (!(Tail is _Add add) || add.Ends != at)
				{
					Tail = add = new _Add(at, _eventArgsOffset, max - at);
				}
				add.Append(item);
			}

			public void Move(object item, int from, int fromOffset, int to, int max)
			{
				if (!(Tail is _Move move) || move.Ends != from)
				{
					Tail = move = new _Move(from + fromOffset, to, _eventArgsOffset, max - to);
				}
				move.Append(item);
			}

			public void Remove(object item, int at)
			{
				if (!(Tail is _Remove remove) || remove.Ends != at)
				{
					Tail = remove = new _Remove(at, _eventArgsOffset, Math.Max(_oldItemsCount - at, 4));
				}
				remove.Append(item);
			}

			private void UpdateOrReplace<T>(ref T? head, object oldItem, object newItem, int index, Func<int, int, T> factory)
				where T : EntityChangeBase
			{
				if (head is null)
				{
					head = factory(index, _eventArgsOffset);
					head.Append(oldItem, newItem);

					return;
				}

				// Search the target node
				var node = head;
				while (node.Next is not null && node.Next.Starts < index)
				{
					node = (T)node.Next;
				}

				// Then append the item to the selected nodes
				if (node.Ends == index)
				{
					node.Append(oldItem, newItem);
				}
				else if (node.Next is null || node.Next.Starts > index)
				{
					// Item is between selected node and the next one, insert a new node.
					var intermediate = factory(index, _eventArgsOffset);
					intermediate.Next = node.Next; // Must be set before Append to allow auto-merge
					node.Next = intermediate;

					intermediate.Append(oldItem, newItem);
				}
			}
		}
	}
}
