import React, { Component } from 'react';
import { Table } from 'antd';
import Moment from 'react-moment';
import moment from 'moment';
import groupBy from 'lodash/groupBy';

const { Column } = Table;

const groupKillmailsByKilledAt = killmails => {
  return groupBy(killmails, (killmail) => {
    return moment(killmail.killedAt).format("LL");
  });
};

class Killmails extends Component {
  render() {
    var killmailsGroupedByKilledAt = groupKillmailsByKilledAt(this.props.killmails);
    return (
      Object.keys(killmailsGroupedByKilledAt).map(dateKey => {
        let killmails = killmailsGroupedByKilledAt[dateKey];
        return (
          <div>
            <h1>{dateKey}</h1>
            <Table dataSource={killmails} rowKey="nodeId" pagination={false}>
              <Column 
                title="Time"
                dataIndex="killedAt"
                key="killedAt"
                render={(text, record) => (
                  // TODO: This needs to be displayed in local time
                  <Moment interval={0} format="LT">{record.killedAt}</Moment>
                )}
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
        )
      })
    );
  }
}

export default Killmails;
