import React, { Component } from 'react';
import { Table } from 'antd';
import gql from 'graphql-tag';

const { Column } = Table;

export const KillmailDataFragment = gql`
fragment KillmailData on Killmail {
  nodeId
  id,
  characterByVictimId {
    id,
    name
  },
  guildByVictimGuildId {
    id, 
    name
  },
  victimLevel,
  characterByAttackerId {
    id,
    name
  },
  guildByAttackerGuildId {
    id,
    name
  },
  attackerLevel,
  zoneByZoneId {
    id,
    name
  },
  killedAt  
}
`;

class Killmails extends Component {
  render() {
    return (
      <div>
        <h1>Jun 21, 2018</h1>
        <Table dataSource={this.props.killmails} rowKey="nodeId" pagination={false}>
          <Column 
            title="Time"
            dataIndex="killedAt"
            key="killedAt"
          />
          <Column
            title="Zone"
            dataIndex="zoneByZoneId"
            key="zoneByZoneId"
            render={(text, record) => (
              <a href="javascript:;">{record.zoneByZoneId.name}</a>
            )}
          /> 
          <Column
            title="Victim"
            dataIndex="characterByVictimId"
            key="characterByVictimId"
            render={(text, record) => (
              <a href="javascript:;">{record.characterByVictimId.name} ({record.victimLevel ? record.victimLevel : 'Unknown level'})</a>
            )}
          />
          <Column
            title="Victim Guild"
            dataIndex="guildByVictimGuildId"
            key="guildByVictimGuildId"
            render={(text, record) => (
              <a href="javascript:;">{record.guildByVictimGuildId ? record.guildByVictimGuildId.name : ''}</a>
            )}
          />        
          <Column
            title="Attacker"
            dataIndex="characterByAttackerId"
            key="characterByAttackerId"
            render={(text, record) => (
              <a href="javascript:;">{record.characterByAttackerId.name} ({record.attackerLevel ? record.attackerLevel : 'Unknown level'})</a>
            )}
          />
          <Column
            title="Attacker Guild"
            dataIndex="guildByAttackerGuildId"
            key="guildByAttackerGuildId"
            render={(text, record) => (
              <a href="javascript:;">{record.guildByAttackerGuildId ? record.guildByAttackerGuildId.name : ''}</a>
            )}
          />     
        </Table>
        <h1>Jun 20, 2018</h1>
        <Table dataSource={this.props.killmails} rowKey="nodeId" pagination={false}>
          <Column 
            title="Time"
            dataIndex="killedAt"
            key="killedAt"
          />
          <Column
            title="Zone"
            dataIndex="zoneByZoneId"
            key="zoneByZoneId"
            render={(text, record) => (
              <a href="javascript:;">{record.zoneByZoneId.name}</a>
            )}
          /> 
          <Column
            title="Victim"
            dataIndex="characterByVictimId"
            key="characterByVictimId"
            render={(text, record) => (
              <a href="javascript:;">{record.characterByVictimId.name} ({record.victimLevel ? record.victimLevel : 'Unknown level'})</a>
            )}
          />
          <Column
            title="Victim Guild"
            dataIndex="guildByVictimGuildId"
            key="guildByVictimGuildId"
            render={(text, record) => (
              <a href="javascript:;">{record.guildByVictimGuildId ? record.guildByVictimGuildId.name : ''}</a>
            )}
          />        
          <Column
            title="Attacker"
            dataIndex="characterByAttackerId"
            key="characterByAttackerId"
            render={(text, record) => (
              <a href="javascript:;">{record.characterByAttackerId.name} ({record.attackerLevel ? record.attackerLevel : 'Unknown level'})</a>
            )}
          />
          <Column
            title="Attacker Guild"
            dataIndex="guildByAttackerGuildId"
            key="guildByAttackerGuildId"
            render={(text, record) => (
              <a href="javascript:;">{record.guildByAttackerGuildId ? record.guildByAttackerGuildId.name : ''}</a>
            )}
          />     
        </Table>
      </div>
    );
  }
}

export default Killmails;
