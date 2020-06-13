import React, { Component } from 'react';
import { Table, Tag, Popover } from 'antd';
import moment from 'moment';
import groupBy from 'lodash/groupBy';
import { Link } from 'react-router-dom';
import { SeasonContext } from './SeasonContext'

const { Column } = Table;
const killColor = "#daf7da";
const deathColor = "#ffe8e7"

class Killmails extends Component {
  static contextType = SeasonContext;

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
    if (this.props.characterId) {
      if (record.characterByVictimId.id.toString() === this.props.characterId) {
        style.backgroundColor = deathColor;
      } else {
        style.backgroundColor = killColor;
      }
    } else if (this.props.guildId) {
      if (record.guildByVictimGuildId && record.guildByVictimGuildId.id.toString() === this.props.guildId) {
        style.backgroundColor = deathColor;
      } else {
        style.backgroundColor = killColor;
      }
    }
    return {
      style: style
    };
  }
  
  othersTagWithPopover = (killmailInvolvedsByKillmailId) => {
    const count = killmailInvolvedsByKillmailId.totalCount;
    const othersTag = () => {
      if (count == 0 || count == 1) {
        return (
          <Tag color="#87d068">Solo</Tag>
        )
      }
      if (count == 2) {
        return (
          <Tag color="geekblue">{count - 1} other</Tag>
        )
      } else {
        return (
          <Tag color="geekblue">{count - 1} others</Tag>
        )
      }
    }
    const content = (
      <div>
        {killmailInvolvedsByKillmailId.nodes.map(node => {
          return (
          <div>{node.characterByAttackerId.name} {node.guildByAttackerGuildId ? `<${node.guildByAttackerGuildId.name}>` : ''}</div>
          )
        })}
      </div>
    );
    return (
      <Popover content={content} trigger="click">
        {othersTag()}
      </Popover>
    )
  }

  render() {
    var killmailsGroupedByKilledAt = this.groupKillmailsByKilledAt(this.props.killmails);
    const season = this.context;
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
                  <Link to={`/${season}/zone/${record.zoneByZoneId.id}`}>{record.zoneByZoneId.name}</Link>
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
                    <Link to={`/${season}/character/${record.characterByVictimId.id}`}>
                      {record.characterByVictimId.name} ({record.victimLevel ? record.victimLevel : '?'})
                    </Link>                    
                  )
                  if (record.guildByVictimGuildId) {
                    guildElement = (
                      <Link to={`/${season}/guild/${record.guildByVictimGuildId.id}`}>
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
                    <Link to={`/${season}/character/${record.characterByAttackerId.id}`}>
                      {record.characterByAttackerId.name} ({record.attackerLevel ? record.attackerLevel : '?'})
                    </Link>                    
                  )
                  if (record.guildByAttackerGuildId) {
                    guildElement = (
                      <Link to={`/${season}/guild/${record.guildByAttackerGuildId.id}`}>
                        {record.guildByAttackerGuildId.name}
                      </Link>
                    )
                  }

                  return (
                    <React.Fragment>
                        {characterElement} {this.othersTagWithPopover(record.killmailInvolvedsByKillmailId)}
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
