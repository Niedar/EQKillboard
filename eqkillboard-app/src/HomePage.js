import React, { Component } from 'react';
import { Link } from 'react-router-dom'
import KillmailsQuery from './KillmailsQuery';
import Killmails from './Killmails';
import { Button, Radio, Icon } from 'antd';


// TODO: turn this into a component
const getPaginationButtons = pageInfo => {
  var nextButton;
  var prevButton;

  if (pageInfo.hasNextPage) {
    nextButton = (
      <Button type="primary">
        <Link to={`/after/${pageInfo.endCursor}`}>
          Forward<Icon type="right" />
        </Link>
      </Button>
    )
  } else {

  }

  if (pageInfo.hasPreviousPage) {
    prevButton = (
      <Button type="primary">
        <Link to={`/before/${pageInfo.startCursor}`}>
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

class HomePage extends Component {
  render() {
    return (
      <KillmailsQuery
        before={this.props.match.params.cursorDirection === "before" ? this.props.match.params.cursor : null}
        after={this.props.match.params.cursorDirection === "after" ? this.props.match.params.cursor : null}
      >
      {({ loading, error, data}) => {
          if (loading) return 'Loading...';
          if (error) return `Error! ${error.message}`;
          
          return (
            <div>
              <Killmails killmails={data.allKillmails.nodes} />
              { getPaginationButtons(data.allKillmails.pageInfo) }
            </div>
          );
      }}
      </KillmailsQuery>
    );
  }
}

export default HomePage;
