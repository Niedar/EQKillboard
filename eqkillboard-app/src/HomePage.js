import React, { Component } from 'react';
import { Link } from 'react-router-dom'
import KillmailsQuery from './KillmailsQuery';
import TopStatsQuery from './TopStatsQuery';
import Killmails from './Killmails';
import TopStats from './TopStats';
import { Button, Icon } from 'antd';
import { Spin, Row, Col } from 'antd';
import { SeasonContext } from './SeasonContext'


// TODO: turn this into a component
const getPaginationButtons = (pageInfo, season) => {
  let buttonStyle = {
    marginTop: "20px",
    marginRight: "20px"
  }
  var nextButton;
  var prevButton;

  nextButton = (
    <Button type="primary" style={buttonStyle} disabled={!pageInfo.hasNextPage}>
      <Link to={`/${season}/after/${pageInfo.endCursor}`} style={{ textDecoration: "none"}}>
        Forward<Icon type="right" />
      </Link>
    </Button>
  )
  prevButton = (
    <Button type="primary" style={buttonStyle} disabled={!pageInfo.hasPreviousPage}>
      <Link to={`/${season}/before/${pageInfo.startCursor}`} style={{ textDecoration: "none"}}>
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

class HomePage extends Component {
  static contextType = SeasonContext;
  render() {
    return (
      <React.Fragment>
        <TopStatsQuery>
        {({ loading, error, data}) => {
            if (loading) return null;
            if (error) return `Error! ${error.message}`;
            
            return (
              <div>
                <h1 style={{marginLeft: "10px"}}>Leaderboard</h1>
                <TopStats allCharacters={data.allCharacters} allGuilds={data.allGuilds} allCharacterRankedKillDeaths={data.allCharacterRankedKillDeaths} allGuildRankedKillDeaths={data.allGuildRankedKillDeaths} allClasses={data.allClasses} />
              </div>
            );
        }}
        </TopStatsQuery>
        <KillmailsQuery
          before={this.props.match.params.cursorDirection === "before" ? this.props.match.params.cursor : null}
          after={this.props.match.params.cursorDirection === "after" ? this.props.match.params.cursor : null}
        >
        {({ loading, error, data}) => {
            if (loading) return <Spin size="large" />;
            if (error) return `Error! ${error.message}`;
            
            return (
              <div>
                <br />
                <h1 style={{marginLeft: "10px"}}>Latest Kills</h1>
                { getPaginationButtons(data.allKillmails.pageInfo, this.context) }
                <Killmails killmails={data.allKillmails.nodes} />
                { getPaginationButtons(data.allKillmails.pageInfo, this.context) }
              </div>
            );
        }}
        </KillmailsQuery>
      </React.Fragment>
    );
  }
}

export default HomePage;
