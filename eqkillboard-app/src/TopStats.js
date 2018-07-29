import React, { Component } from 'react';
import { Table, Row, Col } from 'antd';
import moment from 'moment';
import groupBy from 'lodash/groupBy';
import {Link} from 'react-router-dom';
import orderBy from 'lodash/orderBy';
import take from 'lodash/take';

const { Column } = Table;

class TopStats extends Component {
  render() {
    var topCharacters = take(orderBy(this.props.allCharacters.nodes, [node => {
        return node.killmailsByAttackerId.totalCount;
    }], ["desc"]), 10);
    var topGuilds = take(orderBy(this.props.allGuilds.nodes, [node => {
        return node.killmailsByAttackerGuildId.totalCount;
    }], ["desc"]), 10);

    return (
      <Row gutter={16}>
        <Col span={12}>
          <Table dataSource={topCharacters} rowKey="nodeId" pagination={false} size="small">
            <Column 
              title="Kills"
              dataIndex="killedAt"
              key="killedAt"
              render={(text, record) => (
                // TODO: This needs to be displayed in local time
                record.killmailsByAttackerId.totalCount
              )}
              width={75}
            />
            <Column
              title="Character"
              dataIndex="id"
              key="id"
              render={(text, record) => (
                <a href="javascript:;">{record.name}</a>
              )}
              width={150}
            /> 
          </Table>
        </Col>
        <Col span={12}>
          <Table dataSource={topGuilds} rowKey="nodeId" pagination={false} size="small">
            <Column 
              title="Kills"
              dataIndex="killedAt"
              key="killedAt"
              render={(text, record) => (
                // TODO: This needs to be displayed in local time
                record.killmailsByAttackerGuildId.totalCount
              )}
              width={75}
            />
            <Column
              title="Guild"
              dataIndex="id"
              key="id"
              render={(text, record) => (
                <a href="javascript:;">{record.name}</a>
              )}
              width={150}
            /> 
          </Table>
        </Col>
      </Row>
    );
  }
}

export default TopStats;
