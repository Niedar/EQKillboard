import React, { Component } from 'react';
import { Link } from 'react-router-dom'
import KillmailsQuery from './KillmailsQuery';
import GuildInfoQuery from './GuildInfoQuery';
import Killmails from './Killmails';
import { Button, Icon } from 'antd';
import { Spin, Row, Col } from 'antd';



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

class GuildPage extends Component {
  render() {
    return (
      <React.Fragment>
        <GuildInfoQuery guildId={this.props.match.params.guildId}>
        {({ loading, error, data}) => {
            if (loading) return null;
            if (error) return `Error! ${error.message}`;
            
            return (
              <div style={{marginLeft: "10px"}}>
                <h1>{data.guildById.name}</h1>
                <h2>Kills: {data.guildById.killmailsByAttackerGuildId.totalCount}</h2>
                <h2>Deaths: {data.guildById.killmailsByVictimGuildId.totalCount}</h2>
              </div>
            );
        }}
        </GuildInfoQuery>
        <KillmailsQuery
          before={this.props.match.params.cursorDirection === "before" ? this.props.match.params.cursor : null}
          after={this.props.match.params.cursorDirection === "after" ? this.props.match.params.cursor : null}
          guildId={this.props.match.params.guildId}
        >
        {({ loading, error, data}) => {
            if (loading) return <Spin size="large" />;
            if (error) return `Error! ${error.message}`;
            
            return (
              <div>
                { getPaginationButtons(data.allKillmails.pageInfo, `/guild/${this.props.match.params.guildId}`) }
                <Killmails killmails={data.allKillmails.nodes} guildId={this.props.match.params.guildId}/>
                { getPaginationButtons(data.allKillmails.pageInfo, `/guild/${this.props.match.params.guildId}`) }
              </div>
            );
        }}
        </KillmailsQuery>
      </React.Fragment>
    );
  }
}

export default GuildPage;
