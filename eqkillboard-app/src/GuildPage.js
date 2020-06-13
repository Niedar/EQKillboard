import React, { Component } from 'react';
import { Link } from 'react-router-dom'
import KillmailsQuery from './KillmailsQuery';
import GuildInfoQuery from './GuildInfoQuery';
import Killmails from './Killmails';
import { Button, Icon, Tabs } from 'antd';
import { Spin, Row, Col } from 'antd';
import { SeasonContext } from './SeasonContext';



// TODO: turn this into a component
const getPaginationButtons = (pageInfo, baseUrl) => {
  let buttonStyle = {
    marginTop: "20px",
    marginRight: "20px"
  }
  var nextButton;
  var prevButton;

  nextButton = (
    <Button type="primary" style={buttonStyle} disabled={!pageInfo.hasNextPage}>
      <Link to={`${baseUrl}/after/${pageInfo.endCursor}`} style={{ textDecoration: "none"}}>
        Forward<Icon type="right" />
      </Link>
    </Button>
  )
  prevButton = (
    <Button type="primary" style={buttonStyle} disabled={!pageInfo.hasPreviousPage}>
      <Link to={`${baseUrl}/before/${pageInfo.startCursor}`} style={{ textDecoration: "none"}}>
        <Icon type="left" />Backward
      </Link>
    </Button>
  )

  return (
    <React.Fragment>
      <Row type={"flex"} justify={"end"}>
        <Col>
          {prevButton}
          {nextButton}
        </Col>
      </Row>
    </React.Fragment>
  );
};

function calculateKDR(kills, deaths) {
  return deaths === 0 ? kills.toFixed(1) : (kills / deaths).toFixed(1);
}

class GuildPage extends Component {
  static contextType = SeasonContext;
  render() {
    const season = this.context;
    return (
      <React.Fragment>
        <GuildInfoQuery guildId={parseInt(this.props.match.params.guildId)}>
        {({ loading, error, data}) => {
            if (loading) return null;
            if (error) return `Error! ${error.message}`;
            
            return (
              <div style={{marginLeft: "10px"}}>
                <h1>{data.guildById.name}</h1>
                <Tabs defaultActiveKey="1">
                <Tabs.TabPane tab="Ranked" key="1">
                    <h2>K/D Ratio: {calculateKDR(data.allGuildRankedKillDeathInvolveds.nodes[0].rankedKills, data.allGuildRankedKillDeathInvolveds.nodes[0].rankedDeaths)}</h2>
                    <h2>Kills: {data.allGuildRankedKillDeathInvolveds.nodes[0].rankedKills}</h2>
                    <h2>Deaths: {data.allGuildRankedKillDeathInvolveds.nodes[0].rankedDeaths}</h2>
                  </Tabs.TabPane>
                  <Tabs.TabPane tab="Unranked" key="2">
                    <h2>K/D Ratio: {calculateKDR(data.guildById.kills, data.guildById.killmailsByVictimGuildId.totalCount)}</h2>
                    <h2>Kills: {data.guildById.kills}</h2>
                    <h2>Deaths: {data.guildById.killmailsByVictimGuildId.totalCount}</h2>
                  </Tabs.TabPane>
                </Tabs>
              </div>
            );
        }}
        </GuildInfoQuery>
        <KillmailsQuery
          before={this.props.match.params.cursorDirection === "before" ? this.props.match.params.cursor : null}
          after={this.props.match.params.cursorDirection === "after" ? this.props.match.params.cursor : null}
          guildId={parseInt(this.props.match.params.guildId)}
        >
        {({ loading, error, data}) => {
            if (loading) return <Spin size="large" />;
            if (error) return `Error! ${error.message}`;
            
            return (
              <div>
                { getPaginationButtons(data.allKillmails.pageInfo, `/${season}/guild/${this.props.match.params.guildId}`) }
                <Killmails killmails={data.allKillmails.nodes} guildId={this.props.match.params.guildId}/>
                { getPaginationButtons(data.allKillmails.pageInfo, `/${season}/guild/${this.props.match.params.guildId}`) }
              </div>
            );
        }}
        </KillmailsQuery>
      </React.Fragment>
    );
  }
}

export default GuildPage;
