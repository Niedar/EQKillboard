import React, { Component } from 'react';
import { Table, Row, Col, Tabs, Select} from 'antd';
import {Link} from 'react-router-dom';
import orderBy from 'lodash/orderBy';
import take from 'lodash/take';
import filter from 'lodash/filter';
import { SeasonContext } from './SeasonContext';

const { Column } = Table;

class TopStats extends Component {
  static contextType = SeasonContext;

  constructor(props) {
    super(props);
    this.state = {
      selectedClassId: null
    }
  }
  render() {
    const season = this.context;

    let topCharacters = filter(this.props.allCharacters.nodes, (character) => {
      if (this.state.selectedClassId == null) {
        return true;
      } else {
        return character.classId === this.state.selectedClassId;
      }
    });

    let topRankedCharacters = filter(this.props.allCharacterRankedKillDeathInvolveds.nodes, (character) => {
      if (this.state.selectedClassId == null) {
        return true;
      } else {
        return character.classId === this.state.selectedClassId;
      }
    });


    topCharacters = take(orderBy(topCharacters, [node => {
        return node.killmailInvolvedsByAttackerId.totalCount;
    }], ["desc"]), 10);
    var topGuilds = take(orderBy(this.props.allGuilds.nodes, [node => {
        return node.kills;
    }], ["desc"]), 10);

    topRankedCharacters = take(topRankedCharacters, 10);
    const topRankedGuilds =  take(this.props.allGuildRankedKillDeathInvolveds.nodes, 10);
    const allClasses = this.props.allClasses.nodes;


    console.log(allClasses);
    return (
      <React.Fragment>
      <Select placeholder="All classes" style={{ width: 200, marginLeft: 8}} allowClear={true} onChange={(value) => this.setState({selectedClassId: value})}>
        {allClasses.map(item => (
          <Select.Option key={item.id} value={item.id}>
            {item.name}
          </Select.Option>
        ))}
      </Select>
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
                    record.killmailInvolvedsByAttackerId.totalCount
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
                    record.kills
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
      </React.Fragment>
    );
  }
}

export default TopStats;
