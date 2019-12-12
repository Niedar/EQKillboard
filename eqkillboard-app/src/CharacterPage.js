import React, { Component } from 'react';
import { Link } from 'react-router-dom'
import KillmailsQuery from './KillmailsQuery';
import CharacterInfoQuery from './CharacterInfoQuery';
import Killmails from './Killmails';
import { Button, Icon } from 'antd';
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

class CharacterPage extends Component {
  static contextType = SeasonContext;
  render() {
    const season = this.context;
    return (
      <React.Fragment>
        <CharacterInfoQuery characterId={this.props.match.params.characterId}>
        {({ loading, error, data}) => {
            if (loading) return null;
            if (error) return `Error! ${error.message}`;
            
            let characterGuildHeader;
            if (data.characterById.guildByGuildId) {
              characterGuildHeader = <h1>{data.characterById.name} &lt;{data.characterById.guildByGuildId.name}&gt;</h1>
            } else {
              characterGuildHeader = <h1>{data.characterById.name}</h1>
            }

            return (
              <div style={{marginLeft: "10px"}}>
                {characterGuildHeader}
                <h2>Kills: {data.characterById.killmailsByAttackerId.totalCount}</h2>
                <h2>Deaths: {data.characterById.killmailsByVictimId.totalCount}</h2>
              </div>
            );
        }}
        </CharacterInfoQuery>
        <KillmailsQuery
          before={this.props.match.params.cursorDirection === "before" ? this.props.match.params.cursor : null}
          after={this.props.match.params.cursorDirection === "after" ? this.props.match.params.cursor : null}
          characterId={this.props.match.params.characterId}
        >
        {({ loading, error, data}) => {
            if (loading) return <Spin size="large" />;
            if (error) return `Error! ${error.message}`;
            
            return (
              <div>
                { getPaginationButtons(data.allKillmails.pageInfo, `/${season}/character/${this.props.match.params.characterId}`) }
                <Killmails killmails={data.allKillmails.nodes} characterId={this.props.match.params.characterId}/>
                { getPaginationButtons(data.allKillmails.pageInfo, `/${season}/character/${this.props.match.params.characterId}`) }
              </div>
            );
        }}
        </KillmailsQuery>
      </React.Fragment>
    );
  }
}

export default CharacterPage;
