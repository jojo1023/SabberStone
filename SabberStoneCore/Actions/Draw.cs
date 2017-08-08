﻿using System;
using SabberStoneCore.Model;
using SabberStoneCore.Enums;
using SabberStoneCore.Kettle;

namespace SabberStoneCore.Actions
{
    public partial class Generic
    {
        public static IPlayable DrawCard(Controller c, Card card)
        {
            return DrawCardBlock.Invoke(c, card);
        }

        public static IPlayable Draw(Controller c, IPlayable cardToDraw = null)
        {
            return DrawBlock.Invoke(c, cardToDraw);
        }

        public static Func<Controller, Card, IPlayable> DrawCardBlock
            => delegate(Controller c, Card card)
            {
                var playable = Entity.FromCard(c, card);
                //c.NumCardsDrawnThisTurn++;
                AddHandPhase.Invoke(c, playable);
                return playable;
            };

        public static Func<Controller, IPlayable, IPlayable> DrawBlock
            => delegate(Controller c, IPlayable cardToDraw)
            {
                if (!PreDrawPhase.Invoke(c))
                    return null;

                var playable = DrawPhase.Invoke(c, cardToDraw);
                //c.NumCardsToDraw--; 

                AddHandPhase.Invoke(c, playable);

                return playable;
            };

        private static Func<Controller, bool> PreDrawPhase
            => delegate(Controller c)
            {
                if (c.DeckZone.IsEmpty)
                {
                    var fatigueDamage = c.Hero.Fatigue == 0 ? 1 : c.Hero.Fatigue + 1;
                    DamageCharFunc(c.Hero, c.Hero, fatigueDamage, 0);
                    return false;
                }
                return true;
            };

        private static Func<Controller, IPlayable, IPlayable> DrawPhase
            => delegate(Controller c, IPlayable cardToDraw)
            {
                var playable = c.DeckZone.Remove(cardToDraw ?? c.DeckZone[0]);
                c.Game.Log(LogLevel.INFO, BlockType.ACTION, "DrawPhase", $"{c.Name} draws {playable}");

                c.NumCardsDrawnThisTurn++;
                c.LastCardDrawn = playable.Id;

                return playable;
            };
    }
}