import React, { Component } from 'react';
import { Link } from 'react-router-dom'
import KillmailsQuery from './KillmailsQuery';
import Killmails from './Killmails';
import { Button, Radio, Icon } from 'antd';
import { Spin } from 'antd';



// TODO: turn this into a component
const getPaginationButtons = (pageInfo, baseUrl) => {
  var nextButton;
  var prevButton;

  if (pageInfo.hasNextPage) {
    nextButton = (
      <Button type="primary">
        <Link to={`${baseUrl}/after/${pageInfo.endCursor}`}>
          Forward<Icon type="right" />
        </Link>
      </Button>
    )
  } else {

  }

  if (pageInfo.hasPreviousPage) {
    prevButton = (
      <Button type="primary">
        <Link to={`${baseUrl}/before/${pageInfo.startCursor}`}>
          <Icon type="left" />Backward
        </Link>
      </Button>
    )
  } else {

  }

  return (
    <React.Fragment>
      {prevButton}
      {nextButton}
    </React.Fragment>
  );
};

class CharacterPage extends Component {
  render() {
    return (
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
              <h1>Character {this.props.match.params.characterId}</h1>
              { getPaginationButtons(data.allKillmails.pageInfo, `/character/${this.props.match.params.characterId}`) }
              <Killmails killmails={data.allKillmails.nodes} />
              { getPaginationButtons(data.allKillmails.pageInfo, `/character/${this.props.match.params.characterId}`) }
            </div>
          );
      }}
      </KillmailsQuery>
    );
  }
}

export default CharacterPage;
