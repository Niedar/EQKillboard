import React, { Component } from 'react';
import { Table } from 'antd';
import moment from 'moment';
import groupBy from 'lodash/groupBy';
import { Link } from 'react-router-dom';

const { Column } = Table;
const killColor = "#daf7da";
const deathColor = "#ffe8e7"

class Killmails extends Component {
  groupKillmailsByKilledAt = killmails => {
    return groupBy(killmails, (killmail) => {
      return moment(killmail.killedAt).format("LL");
    });
  };

  getLocalTime = dateTimeWithTimeZone => {
    return moment(dateTimeWithTimeZone).local().format("LT");
  }

  onRow = (record, index) => {
    var style = {};
    console.log(this)
    if (this.props.characterId) {
      if (record.characterByAttackerId.id.toString() === this.props.characterId) {
        style.backgroundColor = killColor;
      } else if (record.characterByVictimId.id.toString() === this.props.characterId) {
        style.backgroundColor = deathColor;
      }
    } else if (this.props.guildId) {
      if (record.guildByAttackerGuildId && record.guildByAttackerGuildId.id.toString() === this.props.guildId) {
        style.backgroundColor = killColor;
      } else if (record.guildByVictimGuildId && record.guildByVictimGuildId.id.toString() === this.props.guildId) {
        style.backgroundColor = deathColor;
      }
    }
    return {
      style: style
    };
  }

  render() {
    var killmailsGroupedByKilledAt = this.groupKillmailsByKilledAt(this.props.killmails);
    return (
      Object.keys(killmailsGroupedByKilledAt).map((dateKey, index) => {
        let killmails = killmailsGroupedByKilledAt[dateKey];
        return (
          <div key={`killmailTableGroup-${index}`}>
            <h2 style={{marginLeft: "10px"}}>{dateKey}</h2>
            <Table dataSource={killmails} rowKey="nodeId" pagination={false} onRow={this.onRow}>
              <Column 
                title="Time"
                dataIndex="killedAt"
                key={`killedAt-${index}`}
                render={(text, record) => (
                  // TODO: This needs to be displayed in local time
                  this.getLocalTime(record.killedAt)
                )}
                width={90}
              />
              <Column
                title="Zone"
                dataIndex="zoneByZoneId"
                key={`zoneByZoneId-${index}`}
                render={(text, record) => (
                  <a href="javascript:;">{record.zoneByZoneId.name}</a>
                )}
                width={150}
              /> 
              <Column
                title="Victim"
                dataIndex="characterByVictimId"
                key={`characterByVictimId-${index}`}
                render={(text, record) => {
                  var characterElement;
                  var guildElement;

                  characterElement = (
                    <Link to={`/character/${record.characterByVictimId.id}`}>
                      {record.characterByVictimId.name} ({record.victimLevel ? record.victimLevel : '?'})
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
                key={`characterByAttackerId-${index}`}
                render={(text, record) => {
                  var characterElement;
                  var guildElement;

                  characterElement = (
                    <Link to={`/character/${record.characterByAttackerId.id}`}>
                      {record.characterByAttackerId.name} ({record.attackerLevel ? record.attackerLevel : '?'})
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
