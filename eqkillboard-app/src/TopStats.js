import React, { Component } from 'react';
import { Table, Row, Col, Tabs} from 'antd';
import {Link} from 'react-router-dom';
import orderBy from 'lodash/orderBy';
import take from 'lodash/take';
import { SeasonContext } from './SeasonContext';

const { Column } = Table;

class TopStats extends Component {
  static contextType = SeasonContext;
  render() {
    const season = this.context;

    var topCharacters = take(orderBy(this.props.allCharacters.nodes, [node => {
        return node.killmailsByAttackerId.totalCount;
    }], ["desc"]), 10);
    var topGuilds = take(orderBy(this.props.allGuilds.nodes, [node => {
        return node.killmailsByAttackerGuildId.totalCount;
    }], ["desc"]), 10);

    const topRankedCharacters = take(this.props.allCharacterRankedKillDeaths.nodes, 10);
    const topRankedGuilds =  take(this.props.allGuildRankedKillDeaths.nodes, 10);

    return (
      <Row gutter={16}>
        <Col span={12}>
          <Tabs defaultActiveKey="1">
            <Tabs.TabPane tab="Ranked kills" key="1">
              <Table dataSource={topRankedCharacters} rowKey="id" pagination={false} size="small">
                <Column 
                  title="Kills"
                  key="characterKilCount"
                  render={(text, record) => (
                    record.rankedKills
                  )}
                  width={75}
                />
                <Column
                  title="Character"
                  key="characterId"
                  render={(text, record) => (
                    <Link to={`/${season}/character/${record.id}`}>
                      {record.name}
                    </Link> 
                  )}
                  width={150}
                /> 
              </Table>
            </Tabs.TabPane>
            <Tabs.TabPane tab="Unranked Kills" key="2">
              <Table dataSource={topCharacters} rowKey="nodeId" pagination={false} size="small">
                <Column 
                  title="Kills"
                  key="characterKilCount"
                  render={(text, record) => (
                    // TODO: This needs to be displayed in local time
                    record.killmailsByAttackerId.totalCount
                  )}
                  width={75}
                />
                <Column
                  title="Character"
                  key="characterId"
                  render={(text, record) => (
                    <Link to={`/${season}/character/${record.id}`}>
                      {record.name}
                    </Link> 
                  )}
                  width={150}
                /> 
              </Table>
            </Tabs.TabPane>
          </Tabs>
        </Col>
        <Col span={12}>
          <Tabs defaultActiveKey="1">
          <Tabs.TabPane tab="Ranked kills" key="1">
              <Table dataSource={topRankedGuilds} rowKey="id" pagination={false} size="small">
                <Column 
                  title="Kills"
                  key="guildKilledCount"
                  render={(text, record) => (
                    record.rankedKills
                  )}
                  width={75}
                />
                <Column
                  title="Guild"
                  key="guildId"
                  render={(text, record) => (
                    <Link to={`/${season}/guild/${record.id}`}>
                      {record.name}
                    </Link> 
                  )}
                  width={150}
                />
              </Table>
            </Tabs.TabPane>
            <Tabs.TabPane tab="Unranked kills" key="2">
              <Table dataSource={topGuilds} rowKey="nodeId" pagination={false} size="small">
                <Column 
                  title="Kills"
                  key="guildKilledCount"
                  render={(text, record) => (
                    // TODO: This needs to be displayed in local time
                    record.killmailsByAttackerGuildId.totalCount
                  )}
                  width={75}
                />
                <Column
                  title="Guild"
                  key="guildId"
                  render={(text, record) => (
                    <Link to={`/${season}/guild/${record.id}`}>
                      {record.name}
                    </Link> 
                  )}
                  width={150}
                />
              </Table>
            </Tabs.TabPane>
          </Tabs>
        </Col>
      </Row>
    );
  }
}

export default TopStats;
