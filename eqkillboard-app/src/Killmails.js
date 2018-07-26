import React, { Component } from 'react';
import { Table } from 'antd';
import moment from 'moment';
import groupBy from 'lodash/groupBy';
import {Link} from 'react-router-dom';

const { Column } = Table;

class Killmails extends Component {
  groupKillmailsByKilledAt = killmails => {
    return groupBy(killmails, (killmail) => {
      return moment(killmail.killedAt).format("LL");
    });
  };

  getLocalTime = dateTimeWithTimeZone => {
    return moment(dateTimeWithTimeZone).local().format("LT");
  }

  render() {
    var killmailsGroupedByKilledAt = this.groupKillmailsByKilledAt(this.props.killmails);
    return (
      Object.keys(killmailsGroupedByKilledAt).map(dateKey => {
        let killmails = killmailsGroupedByKilledAt[dateKey];
        return (
          <div>
            <h2 style={{marginLeft: "10px"}}>{dateKey}</h2>
            <Table dataSource={killmails} rowKey="nodeId" pagination={false}>
              <Column 
                title="Time"
                dataIndex="killedAt"
                key="killedAt"
                render={(text, record) => (
                  // TODO: This needs to be displayed in local time
                  this.getLocalTime(record.killedAt)
                )}
                width={75}
              />
              <Column
                title="Zone"
                dataIndex="zoneByZoneId"
                key="zoneByZoneId"
                render={(text, record) => (
                  <a href="javascript:;">{record.zoneByZoneId.name}</a>
                )}
                width={150}
              /> 
              <Column
                title="Victim"
                dataIndex="characterByVictimId"
                key="characterByVictimId"
                render={(text, record) => {
                  var characterElement;
                  var guildElement;

                  characterElement = (
                    <Link to={`/character/${record.characterByVictimId.id}`}>
                      {record.characterByVictimId.name} ({record.victimLevel ? record.victimLevel : 'Unknown level'})
                    </Link>                    
                  )
                  if (record.guildByVictimGuildId) {
                    guildElement = (
                      <Link to={`/guild/${record.guildByVictimGuildId.id}`}>
                        {record.guildByVictimGuildId.name}
                      </Link>
                    )
                  }
                  return (
                    <React.Fragment>
                      {characterElement}
                      <br />
                      {guildElement}
                    </React.Fragment>    
                )}}
                width={150}
              />
              <Column
                title="Attacker"
                dataIndex="characterByAttackerId"
                key="characterByAttackerId"
                render={(text, record) => {
                  var characterElement;
                  var guildElement;

                  characterElement = (
                    <Link to={`/character/${record.characterByAttackerId.id}`}>
                      {record.characterByAttackerId.name} ({record.attackerLevel ? record.attackerLevel : 'Unknown level'})
                    </Link>                    
                  )
                  if (record.guildByAttackerGuildId) {
                    guildElement = (
                      <Link to={`/guild/${record.guildByAttackerGuildId.id}`}>
                        {record.guildByAttackerGuildId.name}
                      </Link>
                    )
                  }

                  return (
                    <React.Fragment>
                        {characterElement}
                        <br />
                        {guildElement}
                    </React.Fragment>          
                )}}
                width={150}
              />
            </Table>
          </div>
        )
      })
    );
  }
}

export default Killmails;
