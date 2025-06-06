﻿// Copyright (c) CypherCore <http://github.com/CypherCore> All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE. See LICENSE file in the project root for full license information.

using Framework.Constants;
using Game.Entities;
using Game.Networking.Packets;
using System;

namespace Game.Spells
{
    public class SpellCastTargets
    {
        public SpellCastTargets()
        {
            m_strTarget = "";

            m_src = new SpellDestination();
            m_dst = new SpellDestination();
        }

        public SpellCastTargets(Unit caster, SpellCastRequestPkt spellCastRequest)
        {
            m_targetMask = spellCastRequest.Target.Flags;
            m_objectTargetGUID = spellCastRequest.Target.Unit;
            m_itemTargetGUID = spellCastRequest.Target.Item;
            m_strTarget = spellCastRequest.Target.Name;

            m_src = new SpellDestination();
            m_dst = new SpellDestination();

            if (spellCastRequest.Target.SrcLocation != null)
            {
                m_src.TransportGUID = spellCastRequest.Target.SrcLocation.Transport;
                Position pos;
                if (!m_src.TransportGUID.IsEmpty())
                    pos = m_src.TransportOffset;
                else
                    pos = m_src.Position;

                pos.Relocate(spellCastRequest.Target.SrcLocation.Location);
                if (spellCastRequest.Target.Orientation.HasValue)
                    pos.SetOrientation(spellCastRequest.Target.Orientation.Value);
            }

            if (spellCastRequest.Target.DstLocation != null)
            {
                m_dst.TransportGUID = spellCastRequest.Target.DstLocation.Transport;
                Position pos;
                if (!m_dst.TransportGUID.IsEmpty())
                    pos = m_dst.TransportOffset;
                else
                    pos = m_dst.Position;

                pos.Relocate(spellCastRequest.Target.DstLocation.Location);
                if (spellCastRequest.Target.Orientation.HasValue)
                    pos.SetOrientation(spellCastRequest.Target.Orientation.Value);
            }

            SetPitch(spellCastRequest.MissileTrajectory.Pitch);
            SetSpeed(spellCastRequest.MissileTrajectory.Speed);

            Update(caster);
        }

        public void Write(SpellTargetData data)
        {
            data.Flags = m_targetMask;

            if (m_targetMask.HasAnyFlag(SpellCastTargetFlags.Unit | SpellCastTargetFlags.CorpseAlly | SpellCastTargetFlags.Gameobject | SpellCastTargetFlags.CorpseEnemy | SpellCastTargetFlags.UnitMinipet))
                data.Unit = m_objectTargetGUID;

            if (m_targetMask.HasAnyFlag(SpellCastTargetFlags.Item | SpellCastTargetFlags.TradeItem) && m_itemTarget != null)
                data.Item = m_itemTarget.GetGUID();

            if (m_targetMask.HasAnyFlag(SpellCastTargetFlags.SourceLocation))
            {
                TargetLocation target = new();
                target.Transport = m_src.TransportGUID; // relative position guid here - transport for example
                if (!m_src.TransportGUID.IsEmpty())
                    target.Location = m_src.TransportOffset;
                else
                    target.Location = m_src.Position;

                data.SrcLocation = target;
            }

            if (Convert.ToBoolean(m_targetMask & SpellCastTargetFlags.DestLocation))
            {
                TargetLocation target = new();
                target.Transport = m_dst.TransportGUID; // relative position guid here - transport for example
                if (!m_dst.TransportGUID.IsEmpty())
                    target.Location = m_dst.TransportOffset;
                else
                    target.Location = m_dst.Position;

                data.DstLocation = target;
            }

            if (Convert.ToBoolean(m_targetMask & SpellCastTargetFlags.String))
                data.Name = m_strTarget;
        }

        public ObjectGuid GetUnitTargetGUID()
        {
            if (m_objectTargetGUID.IsUnit())
                return m_objectTargetGUID;

            return ObjectGuid.Empty;
        }

        public Unit GetUnitTarget()
        {
            return m_objectTarget?.ToUnit();
        }

        public void SetUnitTarget(Unit target)
        {
            if (target == null)
                return;

            m_objectTarget = target;
            m_objectTargetGUID = target.GetGUID();
            m_targetMask |= SpellCastTargetFlags.Unit;
        }

        ObjectGuid GetGOTargetGUID()
        {
            if (m_objectTargetGUID.IsAnyTypeGameObject())
                return m_objectTargetGUID;

            return ObjectGuid.Empty;
        }

        public GameObject GetGOTarget()
        {
            if (m_objectTarget != null)
                return m_objectTarget.ToGameObject();
            return null;
        }

        public void SetGOTarget(GameObject target)
        {
            if (target == null)
                return;

            m_objectTarget = target;
            m_objectTargetGUID = target.GetGUID();
            m_targetMask |= SpellCastTargetFlags.Gameobject;
        }

        public ObjectGuid GetCorpseTargetGUID()
        {
            if (m_objectTargetGUID.IsCorpse())
                return m_objectTargetGUID;

            return ObjectGuid.Empty;
        }

        public Corpse GetCorpseTarget()
        {
            if (m_objectTarget != null)
                return m_objectTarget.ToCorpse();
            return null;
        }

        public WorldObject GetObjectTarget()
        {
            return m_objectTarget;
        }

        public ObjectGuid GetObjectTargetGUID()
        {
            return m_objectTargetGUID;
        }

        public void RemoveObjectTarget()
        {
            m_objectTarget = null;
            m_objectTargetGUID.Clear();
            m_targetMask &= ~(SpellCastTargetFlags.UnitMask | SpellCastTargetFlags.CorpseMask | SpellCastTargetFlags.GameobjectMask);
        }

        public void SetItemTarget(Item item)
        {
            if (item == null)
                return;

            m_itemTarget = item;
            m_itemTargetGUID = item.GetGUID();
            m_itemTargetEntry = item.GetEntry();
            m_targetMask |= SpellCastTargetFlags.Item;
        }

        public void SetTradeItemTarget(Player caster)
        {
            m_itemTargetGUID = ObjectGuid.TradeItem;
            m_itemTargetEntry = 0;
            m_targetMask |= SpellCastTargetFlags.TradeItem;

            Update(caster);
        }

        public void UpdateTradeSlotItem()
        {
            if (m_itemTarget != null && Convert.ToBoolean(m_targetMask & SpellCastTargetFlags.TradeItem))
            {
                m_itemTargetGUID = m_itemTarget.GetGUID();
                m_itemTargetEntry = m_itemTarget.GetEntry();
            }
        }

        public SpellDestination GetSrc()
        {
            return m_src;
        }

        public Position GetSrcPos()
        {
            return m_src.Position;
        }

        void SetSrc(float x, float y, float z)
        {
            m_src = new SpellDestination(x, y, z);
            m_targetMask |= SpellCastTargetFlags.SourceLocation;
        }

        void SetSrc(Position pos)
        {
            m_src = new SpellDestination(pos);
            m_targetMask |= SpellCastTargetFlags.SourceLocation;
        }

        public void SetSrc(WorldObject wObj)
        {
            m_src = new SpellDestination(wObj);
            m_targetMask |= SpellCastTargetFlags.SourceLocation;
        }

        public void ModSrc(Position pos)
        {
            Cypher.Assert(m_targetMask.HasAnyFlag(SpellCastTargetFlags.SourceLocation));
            m_src.Relocate(pos);
        }

        public void RemoveSrc()
        {
            m_targetMask &= ~SpellCastTargetFlags.SourceLocation;
        }

        public SpellDestination GetDst()
        {
            return m_dst;
        }

        public WorldLocation GetDstPos()
        {
            return m_dst.Position;
        }

        public void SetDst(float x, float y, float z, float orientation, uint mapId = 0xFFFFFFFF)
        {
            m_dst = new SpellDestination(x, y, z, orientation, mapId);
            m_targetMask |= SpellCastTargetFlags.DestLocation;
        }

        public void SetDst(Position pos)
        {
            m_dst = new SpellDestination(pos);
            m_targetMask |= SpellCastTargetFlags.DestLocation;
        }

        public void SetDst(WorldObject wObj)
        {
            m_dst = new SpellDestination(wObj);
            m_targetMask |= SpellCastTargetFlags.DestLocation;
        }

        public void SetDst(SpellDestination spellDest)
        {
            m_dst = spellDest;
            m_targetMask |= SpellCastTargetFlags.DestLocation;
        }

        public void SetDst(SpellCastTargets spellTargets)
        {
            m_dst = spellTargets.m_dst;
            m_targetMask |= SpellCastTargetFlags.DestLocation;
        }

        public void ModDst(Position pos)
        {
            Cypher.Assert(m_targetMask.HasAnyFlag(SpellCastTargetFlags.DestLocation));
            m_dst.Relocate(pos);
        }

        public void ModDst(SpellDestination spellDest)
        {
            Cypher.Assert(m_targetMask.HasAnyFlag(SpellCastTargetFlags.DestLocation));
            m_dst = spellDest;
        }

        public void RemoveDst()
        {
            m_targetMask &= ~SpellCastTargetFlags.DestLocation;
        }

        public void Update(WorldObject caster)
        {
            m_objectTarget = (m_objectTargetGUID == caster.GetGUID()) ? caster : Global.ObjAccessor.GetWorldObject(caster, m_objectTargetGUID);

            m_itemTarget = null;
            if (caster is Player)
            {
                Player player = caster.ToPlayer();
                if (m_targetMask.HasAnyFlag(SpellCastTargetFlags.Item))
                    m_itemTarget = player.GetItemByGuid(m_itemTargetGUID);
                else if (m_targetMask.HasAnyFlag(SpellCastTargetFlags.TradeItem))
                {
                    if (m_itemTargetGUID == ObjectGuid.TradeItem) // here it is not guid but slot. Also prevents hacking slots
                    {
                        TradeData pTrade = player.GetTradeData();
                        if (pTrade != null)
                            m_itemTarget = pTrade.GetTraderData().GetItem(TradeSlots.NonTraded);
                    }
                }

                if (m_itemTarget != null)
                    m_itemTargetEntry = m_itemTarget.GetEntry();
            }

            // update positions by transport move
            if (HasSrc() && !m_src.TransportGUID.IsEmpty())
            {
                WorldObject transport = Global.ObjAccessor.GetWorldObject(caster, m_src.TransportGUID);
                if (transport != null)
                {
                    m_src.Position.Relocate(transport.GetPosition());
                    m_src.Position.RelocateOffset(m_src.TransportOffset);
                }
            }

            if (HasDst() && !m_dst.TransportGUID.IsEmpty())
            {
                WorldObject transport = Global.ObjAccessor.GetWorldObject(caster, m_dst.TransportGUID);
                if (transport != null)
                {
                    m_dst.Position.Relocate(transport.GetPosition());
                    m_dst.Position.RelocateOffset(m_dst.TransportOffset);
                }
            }
        }

        public SpellCastTargetFlags GetTargetMask() { return m_targetMask; }
        public void SetTargetMask(SpellCastTargetFlags newMask) { m_targetMask = newMask; }
        public void SetTargetFlag(SpellCastTargetFlags flag) { m_targetMask |= flag; }

        public ObjectGuid GetItemTargetGUID() { return m_itemTargetGUID; }
        public Item GetItemTarget() { return m_itemTarget; }
        public uint GetItemTargetEntry() { return m_itemTargetEntry; }

        public bool HasSrc() { return Convert.ToBoolean(m_targetMask & SpellCastTargetFlags.SourceLocation); }
        public bool HasDst() { return Convert.ToBoolean(m_targetMask & SpellCastTargetFlags.DestLocation); }
        public bool HasTraj() { return m_speed != 0; }

        public float GetPitch() { return m_pitch; }
        public void SetPitch(float pitch) { m_pitch = pitch; }
        float GetSpeed() { return m_speed; }
        public void SetSpeed(float speed) { m_speed = speed; }

        public float GetDist2d() { return m_src.Position.GetExactDist2d(m_dst.Position); }
        public float GetSpeedXY() { return (float)(m_speed * Math.Cos(m_pitch)); }
        public float GetSpeedZ() { return (float)(m_speed * Math.Sin(m_pitch)); }

        public string GetTargetString() { return m_strTarget; }

        #region Fields
        SpellCastTargetFlags m_targetMask;

        // objects (can be used at spell creating and after Update at casting)
        WorldObject m_objectTarget;
        Item m_itemTarget;

        // object GUID/etc, can be used always
        ObjectGuid m_objectTargetGUID;
        ObjectGuid m_itemTargetGUID;
        uint m_itemTargetEntry;

        SpellDestination m_src;
        SpellDestination m_dst;

        float m_pitch;
        float m_speed;
        string m_strTarget;
        #endregion
    }
}
