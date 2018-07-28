import React, { Component } from "react"
import { Query } from "react-apollo"
import gql from "graphql-tag"

export default class TopStatsQuery extends Component {
  render() {
    const { characterId, children } = this.props

    return (
      <Query query={GET_TOPSTATS}>
        {result => children(result)}
      </Query>
    )
  }
}

const GET_TOPSTATS = gql`
query allStats {
  allCharacters {
    nodes {
      id,
      name,
      killmailsByAttackerId {
        totalCount
      }
    }
  }
  allGuilds {
    nodes {
      killmailsByAttackerGuildId {
        totalCount
      }
    }
  }
}
`;
